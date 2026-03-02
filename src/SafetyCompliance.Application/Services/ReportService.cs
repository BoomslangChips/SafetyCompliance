using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class ReportService(ApplicationDbContext db) : IReportService
{
    // ── Summary ─────────────────────────────────────────────────────────────────

    public async Task<List<PlantReportSummaryDto>> GetPlantSummariesAsync(
        int? companyId = null,
        CancellationToken ct = default)
    {
        var now           = DateOnly.FromDateTime(DateTime.Today);
        var firstOfMonth  = new DateOnly(now.Year, now.Month, 1);
        var lastOfMonth   = firstOfMonth.AddMonths(1).AddDays(-1);

        var plants = await db.Plants
            .Include(p => p.Company)
            .Where(p => p.IsActive && (companyId == null || p.CompanyId == companyId))
            .OrderBy(p => p.Company.Name).ThenBy(p => p.Name)
            .ToListAsync(ct);

        var result = new List<PlantReportSummaryDto>();

        foreach (var plant in plants)
        {
            var pid = plant.Id;

            // Total active equipment
            var totalEquip = await db.Equipment
                .CountAsync(e => e.Section.PlantId == pid && e.IsActive, ct);

            // This-month inspection rounds
            var rounds = await db.InspectionRounds
                .Where(r => r.PlantId == pid
                         && r.InspectionDate >= firstOfMonth
                         && r.InspectionDate <= lastOfMonth)
                .ToListAsync(ct);

            var roundIds  = rounds.Select(r => r.Id).ToList();
            var completed = rounds.Count(r =>
                r.Status == InspectionStatus.Completed ||
                r.Status == InspectionStatus.CompletedWithIssues ||
                r.Status == InspectionStatus.Reviewed);

            var lastDate = rounds.Count > 0 ? rounds.Max(r => r.InspectionDate) : (DateOnly?)null;

            // Distinct equipment inspected and marked complete
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

            // All-time round IDs for this plant (for issue FK lookups)
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

            // Active service bookings for this plant's equipment
            var activeServices = await db.ServiceBookings
                .CountAsync(b =>
                    b.Equipment.Section.PlantId == pid &&
                    (b.Status == ServiceBookingStatus.Sent ||
                     b.Status == ServiceBookingStatus.InService), ct);

            // Notes count for this plant
            var notesCount = await db.Notes
                .CountAsync(n =>
                    n.PlantId == pid ||
                    (n.Equipment != null && n.Equipment.Section.PlantId == pid), ct);

            result.Add(new PlantReportSummaryDto(
                pid, plant.Name, plant.Company.Name,
                completed, rounds.Count,
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
        var plant = await db.Plants
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == plantId, ct);

        if (plant is null) return null;

        var firstOfMonth = new DateOnly(year, month, 1);
        var lastOfMonth  = firstOfMonth.AddMonths(1).AddDays(-1);

        // Total active equipment in plant
        var totalEquip = await db.Equipment
            .CountAsync(e => e.Section.PlantId == plantId && e.IsActive, ct);

        // ── Inspection rounds for the month with full detail ──
        var rounds = await db.InspectionRounds
            .Include(r => r.EquipmentInspections)
                .ThenInclude(ei => ei.Equipment)
                    .ThenInclude(e => e.EquipmentType)
            .Include(r => r.EquipmentInspections)
                .ThenInclude(ei => ei.Equipment)
                    .ThenInclude(e => e.Section)
            .Include(r => r.EquipmentInspections)
                .ThenInclude(ei => ei.Responses)
                    .ThenInclude(resp => resp.ChecklistItemTemplate)
            .Where(r => r.PlantId == plantId
                     && r.InspectionDate >= firstOfMonth
                     && r.InspectionDate <= lastOfMonth)
            .OrderBy(r => r.InspectionDate)
            .ToListAsync(ct);

        // Resolve inspector display names from ASP.NET Identity users
        var inspectorIds = rounds.Select(r => r.InspectedById).Distinct().ToList();
        var userNames = await db.Users
            .Where(u => inspectorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? u.Id, ct);

        // Build round rows + equipment rows
        var roundRows = new List<ReportRoundRowDto>();
        var equipRows = new List<ReportEquipmentRowDto>();

        foreach (var r in rounds)
        {
            var inspectorName = userNames.GetValueOrDefault(r.InspectedById, r.InspectedById);
            var failedChecks  = r.EquipmentInspections.Sum(ei => ei.Responses.Count(resp => resp.Response == false));
            var totalChecks   = r.EquipmentInspections.Sum(ei => ei.Responses.Count(resp => resp.Response.HasValue));

            roundRows.Add(new ReportRoundRowDto(
                r.Id, r.InspectionDate, r.Status.ToString(), inspectorName,
                r.EquipmentInspections.Count(ei => ei.IsComplete),
                r.EquipmentInspections.Count,
                failedChecks, totalChecks, r.CompletedAt));

            foreach (var ei in r.EquipmentInspections
                         .OrderBy(x => x.Equipment.Section.SortOrder)
                         .ThenBy(x => x.Equipment.SortOrder))
            {
                var pass   = ei.Responses.Count(resp => resp.Response == true);
                var fail   = ei.Responses.Count(resp => resp.Response == false);
                var failed = ei.Responses
                    .Where(resp => resp.Response == false)
                    .Select(resp => resp.ChecklistItemTemplate?.ItemName ?? "—")
                    .ToList();

                equipRows.Add(new ReportEquipmentRowDto(
                    r.Id,
                    ei.EquipmentId,
                    ei.Equipment.Identifier,
                    ei.Equipment.EquipmentType.Name,
                    ei.Equipment.Section.Name,
                    ei.IsComplete,
                    pass, fail, failed,
                    ei.Comments));
            }
        }

        // Overall compliance % for the month
        var totalPassed       = equipRows.Sum(e => e.PassChecks);
        var totalChecksMonth  = equipRows.Sum(e => e.PassChecks + e.FailChecks);
        var compliancePct     = totalChecksMonth > 0
            ? (int)Math.Round(totalPassed * 100.0 / totalChecksMonth)
            : 100;

        var roundIds = rounds.Select(r => r.Id).ToList();

        // ── Issues: linked to rounds in this month OR to plant equipment ──
        var issues = await db.Issues
            .Include(i => i.Equipment)
            .Where(i =>
                (roundIds.Count > 0 && i.InspectionRoundId.HasValue
                    && roundIds.Contains(i.InspectionRoundId.Value)) ||
                (i.Equipment != null && i.Equipment.Section.PlantId == plantId))
            .OrderBy(i => i.Status)
            .ThenByDescending(i => i.Priority)
            .ThenByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

        var issueRows = issues
            .Select(i => new ReportIssueRowDto(
                i.Id, i.Title, i.Priority.ToString(), i.Status.ToString(),
                i.AssignedTo, i.DueDate, i.Equipment?.Identifier,
                i.CreatedAt, i.ResolvedAt))
            .ToList();

        // ── Service bookings: sent/returned this month OR currently active ──
        var bookings = await db.ServiceBookings
            .Include(b => b.Equipment).ThenInclude(e => e.EquipmentType)
            .Include(b => b.Equipment).ThenInclude(e => e.Section)
            .Where(b =>
                b.Equipment.Section.PlantId == plantId &&
                ((b.SentDate >= firstOfMonth && b.SentDate <= lastOfMonth) ||
                 (b.ActualReturnDate >= firstOfMonth && b.ActualReturnDate <= lastOfMonth) ||
                 b.Status == ServiceBookingStatus.Sent ||
                 b.Status == ServiceBookingStatus.InService))
            .OrderBy(b => b.Status)
            .ThenByDescending(b => b.SentDate)
            .ToListAsync(ct);

        var bookingRows = bookings
            .Select(b => new ReportServiceRowDto(
                b.Id,
                b.Equipment.Identifier,
                b.Equipment.EquipmentType.Name,
                b.Equipment.Section.Name,
                b.ServiceProvider, b.Reason,
                b.Status.ToString(),
                b.SentDate,
                b.ExpectedReturnDate,
                b.ActualReturnDate))
            .ToList();

        // ── Notes: linked to this plant or its equipment ──
        var notes = await db.Notes
            .Include(n => n.Equipment)
            .Where(n =>
                n.PlantId == plantId ||
                (n.Equipment != null && n.Equipment.Section.PlantId == plantId))
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.Priority)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

        var noteRows = notes
            .Select(n => new ReportNoteRowDto(
                n.Id, n.Title, n.Content,
                n.Category.ToString(), n.Priority.ToString(),
                n.IsPinned, n.Equipment?.Identifier,
                n.CreatedAt))
            .ToList();

        return new MonthlyReportDto(
            plant.Id, plant.Name, plant.Company.Name,
            plant.ContactName, plant.ContactPhone,
            year, month,
            totalEquip, compliancePct,
            roundRows, equipRows,
            issueRows, bookingRows, noteRows,
            DateTime.Now);
    }
}
