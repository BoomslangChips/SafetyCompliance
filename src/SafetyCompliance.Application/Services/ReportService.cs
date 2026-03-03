using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

/// <summary>
/// All queries use SELECT projections instead of Include() chains to avoid
/// SqlNullValueException on non-nullable C# properties that contain NULL in
/// legacy database rows (e.g. CreatedById, Name fields inserted manually).
/// </summary>
public class ReportService(ApplicationDbContext db) : IReportService
{
    // ── Summary ─────────────────────────────────────────────────────────────────

    public async Task<List<PlantReportSummaryDto>> GetPlantSummariesAsync(
        int? companyId = null,
        CancellationToken ct = default)
    {
        var now          = DateOnly.FromDateTime(DateTime.Today);
        var firstOfMonth = new DateOnly(now.Year, now.Month, 1);
        var lastOfMonth  = firstOfMonth.AddMonths(1).AddDays(-1);

        // Project only the fields we need — avoids null reads on CreatedById etc.
        var plants = await db.Plants
            .Where(p => p.IsActive && (companyId == null || p.CompanyId == companyId))
            .Select(p => new
            {
                p.Id,
                Name        = (string?)(p.Name)        ?? "",
                CompanyName = (string?)(p.Company.Name) ?? "",
            })
            .OrderBy(p => p.CompanyName).ThenBy(p => p.Name)
            .ToListAsync(ct);

        var result = new List<PlantReportSummaryDto>();

        foreach (var plant in plants)
        {
            var pid = plant.Id;

            // Total active equipment
            var totalEquip = await db.Equipment
                .CountAsync(e => e.Section.PlantId == pid && e.IsActive, ct);

            // This-month rounds — project only Id, Status, Date
            var monthRounds = await db.InspectionRounds
                .Where(r => r.PlantId == pid
                         && r.InspectionDate >= firstOfMonth
                         && r.InspectionDate <= lastOfMonth)
                .Select(r => new { r.Id, r.Status, r.InspectionDate })
                .ToListAsync(ct);

            var roundIds  = monthRounds.Select(r => r.Id).ToList();
            var completed = monthRounds.Count(r =>
                r.Status == InspectionStatus.Completed ||
                r.Status == InspectionStatus.CompletedWithIssues ||
                r.Status == InspectionStatus.Reviewed);

            var lastDate = monthRounds.Count > 0
                ? monthRounds.Max(r => r.InspectionDate)
                : (DateOnly?)null;

            // Distinct equipment inspected and marked complete this month
            var equipInspected = roundIds.Count > 0
                ? await db.EquipmentInspections
                    .Where(ei => roundIds.Contains(ei.InspectionRoundId) && ei.IsComplete)
                    .Select(ei => ei.EquipmentId)
                    .Distinct()
                    .CountAsync(ct)
                : 0;

            // Compliance % for the month
            var respStats = roundIds.Count > 0
                ? await db.InspectionResponses
                    .Where(r => roundIds.Contains(r.EquipmentInspection.InspectionRoundId)
                             && r.Response.HasValue)
                    .GroupBy(_ => 1)
                    .Select(g => new { Total = g.Count(), Passed = g.Count(r => r.Response == true) })
                    .FirstOrDefaultAsync(ct)
                : null;

            var compliancePct = respStats is { Total: > 0 }
                ? (int)Math.Round(respStats.Passed * 100.0 / respStats.Total)
                : 100;

            // All-time round IDs for this plant (issue FK lookups)
            var allRoundIds = await db.InspectionRounds
                .Where(r => r.PlantId == pid)
                .Select(r => r.Id)
                .ToListAsync(ct);

            // Open incidents linked to this plant
            var openIncidents = await db.Issues
                .CountAsync(i =>
                    ((i.InspectionRoundId.HasValue && allRoundIds.Contains(i.InspectionRoundId.Value)) ||
                     (i.Equipment != null && i.Equipment.Section.PlantId == pid)) &&
                    (i.Status == IssueStatus.Open || i.Status == IssueStatus.InProgress), ct);

            var criticalIncidents = await db.Issues
                .CountAsync(i =>
                    ((i.InspectionRoundId.HasValue && allRoundIds.Contains(i.InspectionRoundId.Value)) ||
                     (i.Equipment != null && i.Equipment.Section.PlantId == pid)) &&
                    i.Priority == IssuePriority.Critical &&
                    (i.Status == IssueStatus.Open || i.Status == IssueStatus.InProgress), ct);

            // Active service bookings
            var activeServices = await db.ServiceBookings
                .CountAsync(b =>
                    b.Equipment.Section.PlantId == pid &&
                    (b.Status == ServiceBookingStatus.Sent ||
                     b.Status == ServiceBookingStatus.InService), ct);

            // Notes count
            var notesCount = await db.Notes
                .CountAsync(n =>
                    n.PlantId == pid ||
                    (n.Equipment != null && n.Equipment.Section.PlantId == pid), ct);

            result.Add(new PlantReportSummaryDto(
                pid, plant.Name, plant.CompanyName,
                completed, monthRounds.Count,
                equipInspected, totalEquip,
                openIncidents, criticalIncidents,
                activeServices, notesCount,
                compliancePct, lastDate));
        }

        return result;
    }

    // ── Monthly Report ───────────────────────────────────────────────────────────

    public async Task<MonthlyReportDto?> GetMonthlyReportAsync(
        int plantId, int year, int month,
        CancellationToken ct = default)
    {
        // Project plant — avoids null reads on AuditableEntity fields
        var plant = await db.Plants
            .Where(p => p.Id == plantId)
            .Select(p => new
            {
                p.Id,
                Name         = (string?)(p.Name)         ?? "",
                CompanyName  = (string?)(p.Company.Name)  ?? "",
                p.ContactName,
                p.ContactPhone,
            })
            .FirstOrDefaultAsync(ct);

        if (plant is null) return null;

        var firstOfMonth = new DateOnly(year, month, 1);
        var lastOfMonth  = firstOfMonth.AddMonths(1).AddDays(-1);

        // Total active equipment
        var totalEquip = await db.Equipment
            .CountAsync(e => e.Section.PlantId == plantId && e.IsActive, ct);

        // ── Inspection rounds — projection (counts computed in SQL) ──
        var roundProj = await db.InspectionRounds
            .Where(r => r.PlantId == plantId
                     && r.InspectionDate >= firstOfMonth
                     && r.InspectionDate <= lastOfMonth)
            .Select(r => new
            {
                r.Id,
                r.InspectionDate,
                r.Status,
                r.InspectedById,
                r.CompletedAt,
                TotalEquipment     = r.EquipmentInspections.Count,
                CompletedEquipment = r.EquipmentInspections.Count(ei => ei.IsComplete),
                FailedChecks       = r.EquipmentInspections
                                      .SelectMany(ei => ei.Responses)
                                      .Count(resp => resp.Response == false),
                TotalChecks        = r.EquipmentInspections
                                      .SelectMany(ei => ei.Responses)
                                      .Count(resp => resp.Response.HasValue),
            })
            .OrderBy(r => r.InspectionDate)
            .ToListAsync(ct);

        var roundIds = roundProj.Select(r => r.Id).ToList();

        // Resolve inspector display names
        var inspectorIds = roundProj.Select(r => r.InspectedById).Distinct().ToList();
        var userNames = await db.Users
            .Where(u => inspectorIds.Contains(u.Id))
            .Select(u => new { u.Id, DisplayName = (string?)u.UserName ?? (string?)u.Email ?? u.Id })
            .ToDictionaryAsync(u => u.Id!, u => u.DisplayName ?? "", ct);

        var roundRows = roundProj.Select(r => new ReportRoundRowDto(
            r.Id,
            r.InspectionDate,
            r.Status.ToString(),
            userNames.GetValueOrDefault(r.InspectedById ?? "", r.InspectedById ?? ""),
            r.CompletedEquipment,
            r.TotalEquipment,
            r.FailedChecks,
            r.TotalChecks,
            r.CompletedAt)).ToList();

        // ── Equipment inspections — projection with pass/fail counts ──
        var equipProj = roundIds.Count > 0
            ? await db.EquipmentInspections
                .Where(ei => roundIds.Contains(ei.InspectionRoundId))
                .Select(ei => new
                {
                    ei.Id,
                    ei.InspectionRoundId,
                    ei.EquipmentId,
                    Identifier   = (string?)(ei.Equipment.Identifier)           ?? "",
                    TypeName     = (string?)(ei.Equipment.EquipmentType.Name)    ?? "",
                    SectionName  = (string?)(ei.Equipment.Section.Name)          ?? "",
                    SectionSort  = ei.Equipment.Section.SortOrder,
                    EquipSort    = ei.Equipment.SortOrder,
                    ei.IsComplete,
                    ei.Comments,
                    PassChecks   = ei.Responses.Count(r => r.Response == true),
                    FailChecks   = ei.Responses.Count(r => r.Response == false),
                })
                .OrderBy(ei => ei.SectionSort).ThenBy(ei => ei.EquipSort)
                .ToListAsync(ct)
            : [];

        // Fetch failed checklist item names in one query (only for rows with failures)
        var equipIdsWithFails = equipProj.Where(e => e.FailChecks > 0).Select(e => e.Id).ToList();
        Dictionary<int, List<string>> failedItemsMap = [];
        if (equipIdsWithFails.Count > 0)
        {
            var failedRows = await db.InspectionResponses
                .Where(r => equipIdsWithFails.Contains(r.EquipmentInspectionId)
                         && r.Response == false)
                .Select(r => new
                {
                    r.EquipmentInspectionId,
                    ItemName = (string?)(r.ChecklistItemTemplate.ItemName) ?? "—"
                })
                .ToListAsync(ct);

            failedItemsMap = failedRows
                .GroupBy(r => r.EquipmentInspectionId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.ItemName).ToList());
        }

        var equipRows = equipProj.Select(ei => new ReportEquipmentRowDto(
            ei.InspectionRoundId,
            ei.EquipmentId,
            ei.Identifier,
            ei.TypeName,
            ei.SectionName,
            ei.IsComplete,
            ei.PassChecks,
            ei.FailChecks,
            failedItemsMap.GetValueOrDefault(ei.Id, []),
            ei.Comments)).ToList();

        // Overall compliance %
        var totalPassed      = equipRows.Sum(e => e.PassChecks);
        var totalChecksMonth = equipRows.Sum(e => e.PassChecks + e.FailChecks);
        var compliancePct    = totalChecksMonth > 0
            ? (int)Math.Round(totalPassed * 100.0 / totalChecksMonth)
            : 100;

        // ── Issues — projection ──
        var issueRows = await db.Issues
            .Where(i =>
                (roundIds.Count > 0 && i.InspectionRoundId.HasValue
                    && roundIds.Contains(i.InspectionRoundId.Value)) ||
                (i.Equipment != null && i.Equipment.Section.PlantId == plantId))
            .OrderBy(i => i.Status)
            .ThenByDescending(i => i.Priority)
            .ThenByDescending(i => i.CreatedAt)
            .Select(i => new ReportIssueRowDto(
                i.Id,
                (string?)(i.Title)             ?? "",
                i.Priority.ToString(),
                i.Status.ToString(),
                i.AssignedTo,
                i.DueDate,
                (string?)(i.Equipment != null ? i.Equipment.Identifier : null),
                i.CreatedAt,
                i.ResolvedAt))
            .ToListAsync(ct);

        // ── Service bookings — projection ──
        var bookingRows = await db.ServiceBookings
            .Where(b =>
                b.Equipment.Section.PlantId == plantId &&
                ((b.SentDate >= firstOfMonth && b.SentDate <= lastOfMonth) ||
                 (b.ActualReturnDate >= firstOfMonth && b.ActualReturnDate <= lastOfMonth) ||
                 b.Status == ServiceBookingStatus.Sent ||
                 b.Status == ServiceBookingStatus.InService))
            .OrderBy(b => b.Status)
            .ThenByDescending(b => b.SentDate)
            .Select(b => new ReportServiceRowDto(
                b.Id,
                (string?)(b.Equipment.Identifier)        ?? "",
                (string?)(b.Equipment.EquipmentType.Name) ?? "",
                (string?)(b.Equipment.Section.Name)       ?? "",
                (string?)(b.ServiceProvider)              ?? "",
                (string?)(b.Reason)                       ?? "",
                b.Status.ToString(),
                b.SentDate,
                b.ExpectedReturnDate,
                b.ActualReturnDate))
            .ToListAsync(ct);

        // ── Notes — projection ──
        var noteRows = await db.Notes
            .Where(n =>
                n.PlantId == plantId ||
                (n.Equipment != null && n.Equipment.Section.PlantId == plantId))
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.Priority)
            .ThenByDescending(n => n.CreatedAt)
            .Select(n => new ReportNoteRowDto(
                n.Id,
                (string?)(n.Title)   ?? "",
                (string?)(n.Content) ?? "",
                n.Category.ToString(),
                n.Priority.ToString(),
                n.IsPinned,
                (string?)(n.Equipment != null ? n.Equipment.Identifier : null),
                n.CreatedAt))
            .ToListAsync(ct);

        // ── Contacts — all contacts for this plant ──
        var contactRows = await db.PlantContacts
            .Where(c => c.PlantId == plantId)
            .OrderBy(c => c.Category)
            .ThenByDescending(c => c.IsPrimary)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ReportContactRowDto(
                c.Id,
                (string?)(c.Category) ?? "",
                (string?)(c.Name)     ?? "",
                c.Role,
                c.Phone,
                c.Email,
                c.Notes,
                c.IsPrimary))
            .ToListAsync(ct);

        return new MonthlyReportDto(
            plant.Id, plant.Name, plant.CompanyName,
            plant.ContactName, plant.ContactPhone,
            year, month,
            totalEquip, compliancePct,
            roundRows, equipRows,
            issueRows, bookingRows, noteRows,
            contactRows,
            DateTime.Now);
    }
}
