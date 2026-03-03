using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class InspectionService(ApplicationDbContext context) : IInspectionService
{
    public async Task<List<InspectionRoundDto>> GetInspectionRoundsAsync(int plantId, CancellationToken ct = default)
    {
        return await context.InspectionRounds
            .Where(ir => ir.PlantId == plantId)
            .OrderByDescending(ir => ir.InspectionDate)
            .Select(ir => new InspectionRoundDto(
                ir.Id, ir.PlantId, ir.Plant.Name, ir.InspectionDate, ir.InspectionMonth,
                ir.Status, ir.InspectedById,
                ir.EquipmentInspections.Count,
                ir.EquipmentInspections.Count(ei => ei.IsComplete),
                ir.CompletedAt))
            .ToListAsync(ct);
    }

    public async Task<InspectionRoundDto> StartInspectionRoundAsync(int plantId, string userId, int? scheduleId = null, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var month = today.ToString("yyyy-MM");

        // Only block if there's an active (Draft/InProgress) round for this month
        var hasActiveRound = await context.InspectionRounds
            .AnyAsync(ir => ir.PlantId == plantId && ir.InspectionMonth == month
                && (ir.Status == InspectionStatus.Draft || ir.Status == InspectionStatus.InProgress), ct);

        if (hasActiveRound)
            throw new InvalidOperationException($"An active inspection round already exists for {month}");

        var round = new InspectionRound
        {
            PlantId = plantId,
            InspectionScheduleId = scheduleId,
            InspectionDate = today,
            InspectionMonth = month,
            Status = InspectionStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            InspectedById = userId
        };

        context.InspectionRounds.Add(round);
        await context.SaveChangesAsync(ct);

        var activeEquipment = await context.Equipment
            .Where(e => e.Section.PlantId == plantId && e.IsActive && e.Section.IsActive)
            .ToListAsync(ct);

        foreach (var eq in activeEquipment)
        {
            context.EquipmentInspections.Add(new EquipmentInspection
            {
                InspectionRoundId = round.Id,
                EquipmentId = eq.Id
            });
        }

        await context.SaveChangesAsync(ct);

        var equipmentInspections = await context.EquipmentInspections
            .Where(ei => ei.InspectionRoundId == round.Id)
            .Include(ei => ei.Equipment)
            .ToListAsync(ct);

        foreach (var ei in equipmentInspections)
        {
            var typeId    = ei.Equipment.EquipmentTypeId;
            var subTypeId = ei.Equipment.EquipmentSubTypeId;

            // Load both type-level (SubTypeId=null) AND sub-type-specific items in one hit,
            // then decide in memory which set to use:
            //   • If sub-type-specific items exist for this equipment's sub-type → use those
            //   • Otherwise fall back to the type-level (null) items
            // This lets DCP and CO2 extinguishers share one equipment type but have
            // completely different inspection checklists.
            var allCandidates = await context.ChecklistItemTemplates
                .Where(c => c.EquipmentTypeId == typeId && c.IsActive &&
                            (c.EquipmentSubTypeId == null || c.EquipmentSubTypeId == subTypeId))
                .ToListAsync(ct);

            var hasSubTypeSpecific = subTypeId.HasValue &&
                                     allCandidates.Any(c => c.EquipmentSubTypeId == subTypeId);

            var checklistItems = hasSubTypeSpecific
                ? allCandidates.Where(c => c.EquipmentSubTypeId == subTypeId).ToList()
                : allCandidates.Where(c => c.EquipmentSubTypeId == null).ToList();

            foreach (var item in checklistItems)
            {
                context.InspectionResponses.Add(new InspectionResponse
                {
                    EquipmentInspectionId   = ei.Id,
                    ChecklistItemTemplateId = item.Id
                });
            }
        }

        await context.SaveChangesAsync(ct);

        return (await GetInspectionRoundsAsync(plantId, ct)).First(r => r.Id == round.Id);
    }

    public async Task<List<EquipmentInspectionDto>> GetEquipmentInspectionsAsync(int roundId, CancellationToken ct = default)
    {
        // Load inspections
        var inspections = await context.EquipmentInspections
            .Where(ei => ei.InspectionRoundId == roundId)
            .OrderBy(ei => ei.Equipment.Section.SortOrder)
            .ThenBy(ei => ei.Equipment.EquipmentType.Name)
            .ThenBy(ei => ei.Equipment.SortOrder)
            .Select(ei => new EquipmentInspectionDto(
                ei.Id, ei.InspectionRoundId, ei.EquipmentId,
                ei.Equipment.Identifier, ei.Equipment.Description ?? "",
                ei.Equipment.EquipmentTypeId,
                ei.Equipment.EquipmentType.Name,
                ei.Equipment.EquipmentSubType != null ? ei.Equipment.EquipmentSubType.Name : null,
                ei.Equipment.Size, ei.IsComplete, ei.Comments,
                ei.Responses
                    .OrderBy(r => r.ChecklistItemTemplate.SortOrder)
                    .Select(r => new InspectionResponseDto(
                        r.Id, r.ChecklistItemTemplateId, r.ChecklistItemTemplate.ItemName,
                        r.ChecklistItemTemplate.SortOrder, r.Response, r.Comment))
                    .ToList(),
                ei.Photos.Count,
                ei.Equipment.Section.Id,
                ei.Equipment.Section.Name,
                (ActiveServiceBookingDto?)null))
            .ToListAsync(ct);

        // Load active service bookings separately to avoid complex subquery
        var equipmentIds = inspections.Select(i => i.EquipmentId).Distinct().ToList();
        var activeBookings = await context.ServiceBookings
            .Where(sb => equipmentIds.Contains(sb.EquipmentId)
                && (sb.Status == ServiceBookingStatus.Sent || sb.Status == ServiceBookingStatus.InService))
            .GroupBy(sb => sb.EquipmentId)
            .Select(g => g.OrderByDescending(sb => sb.SentDate).First())
            .ToDictionaryAsync(sb => sb.EquipmentId, ct);

        // Merge active bookings into DTOs
        if (activeBookings.Count > 0)
        {
            inspections = inspections.Select(i =>
                activeBookings.TryGetValue(i.EquipmentId, out var booking)
                    ? i with { ActiveServiceBooking = new ActiveServiceBookingDto(
                        booking.Id, booking.ServiceProvider, booking.Status,
                        booking.SentDate, booking.ExpectedReturnDate) }
                    : i)
                .ToList();
        }

        return inspections;
    }

    public async Task SubmitResponseAsync(SubmitResponseDto dto, CancellationToken ct = default)
    {
        var response = await context.InspectionResponses
            .FirstOrDefaultAsync(r =>
                r.EquipmentInspectionId == dto.EquipmentInspectionId &&
                r.ChecklistItemTemplateId == dto.ChecklistItemTemplateId, ct)
            ?? throw new InvalidOperationException("Response not found");

        response.Response = dto.Response;
        response.Comment = dto.Comment;

        var allAnswered = !await context.InspectionResponses
            .Where(r => r.EquipmentInspectionId == dto.EquipmentInspectionId && r.Response == null)
            .AnyAsync(r => r.Id != response.Id, ct);

        if (allAnswered)
        {
            var inspection = await context.EquipmentInspections.FindAsync([dto.EquipmentInspectionId], ct);
            if (inspection is not null)
            {
                inspection.IsComplete = true;
                inspection.InspectedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task SaveEquipmentCommentAsync(int equipmentInspectionId, string? comments, CancellationToken ct = default)
    {
        var inspection = await context.EquipmentInspections.FindAsync([equipmentInspectionId], ct)
            ?? throw new InvalidOperationException($"Equipment inspection {equipmentInspectionId} not found");

        inspection.Comments = comments;
        await context.SaveChangesAsync(ct);
    }

    public async Task CompleteRoundAsync(int roundId, CancellationToken ct = default)
    {
        var round = await context.InspectionRounds.FindAsync([roundId], ct)
            ?? throw new InvalidOperationException($"Round {roundId} not found");

        // Check if any equipment is incomplete or has failed responses
        var hasIncomplete = await context.EquipmentInspections
            .AnyAsync(ei => ei.InspectionRoundId == roundId && !ei.IsComplete, ct);

        var hasFailures = await context.InspectionResponses
            .AnyAsync(r => r.EquipmentInspection.InspectionRoundId == roundId && r.Response == false, ct);

        round.Status = (hasIncomplete || hasFailures)
            ? InspectionStatus.CompletedWithIssues
            : InspectionStatus.Completed;
        round.CompletedAt = DateTime.UtcNow;

        // Advance linked schedule's NextDueDate and LastCompletedDate
        if (round.InspectionScheduleId.HasValue)
        {
            var schedule = await context.InspectionSchedules.FindAsync([round.InspectionScheduleId.Value], ct);
            if (schedule is not null)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                schedule.LastCompletedDate = today;
                schedule.NextDueDate = CalculateNextDueDate(schedule.NextDueDate, schedule.Frequency, schedule.FrequencyInterval, today);
                schedule.ModifiedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private static DateOnly CalculateNextDueDate(DateOnly currentDue, FrequencyType frequency, int interval, DateOnly today)
    {
        // Start from whichever is later — the current due date or today
        var baseDate = currentDue >= today ? currentDue : today;

        return frequency switch
        {
            FrequencyType.Daily => baseDate.AddDays(1 * interval),
            FrequencyType.Weekly => baseDate.AddDays(7 * interval),
            FrequencyType.BiWeekly => baseDate.AddDays(14 * interval),
            FrequencyType.Monthly => baseDate.AddMonths(1 * interval),
            FrequencyType.Quarterly => baseDate.AddMonths(3 * interval),
            FrequencyType.SemiAnnually => baseDate.AddMonths(6 * interval),
            FrequencyType.Annually => baseDate.AddYears(1 * interval),
            _ => baseDate.AddMonths(1 * interval)
        };
    }

    public async Task<int> UploadPhotoAsync(int equipmentInspectionId, string fileName, string filePath,
        string contentType, long fileSize, string userId, CancellationToken ct = default)
    {
        var photo = new InspectionPhoto
        {
            EquipmentInspectionId = equipmentInspectionId,
            FileName = fileName,
            FilePath = filePath,
            ContentType = contentType,
            FileSizeBytes = fileSize,
            UploadedById = userId
        };

        context.InspectionPhotos.Add(photo);
        await context.SaveChangesAsync(ct);

        return photo.Id;
    }

    public async Task<List<InspectionPhotoDto>> GetPhotosAsync(int equipmentInspectionId, CancellationToken ct = default)
    {
        return await context.InspectionPhotos
            .Where(p => p.EquipmentInspectionId == equipmentInspectionId)
            .OrderBy(p => p.UploadedAt)
            .Select(p => new InspectionPhotoDto(
                p.Id, p.EquipmentInspectionId, p.FileName,
                p.FilePath, p.ContentType, p.FileSizeBytes,
                p.UploadedAt, p.UploadedById))
            .ToListAsync(ct);
    }

    public async Task<string?> DeletePhotoAsync(int photoId, CancellationToken ct = default)
    {
        var photo = await context.InspectionPhotos.FindAsync([photoId], ct);
        if (photo is null) return null;
        var filePath = photo.FilePath;
        context.InspectionPhotos.Remove(photo);
        await context.SaveChangesAsync(ct);
        return filePath;
    }

    public async Task<List<InspectionRoundDto>> GetActiveRoundsAsync(CancellationToken ct = default)
    {
        return await context.InspectionRounds
            .Where(ir => ir.Status == InspectionStatus.InProgress
                      || ir.Status == InspectionStatus.Draft
                      || ir.Status == InspectionStatus.CompletedWithIssues)
            .OrderByDescending(ir => ir.InspectionDate)
            .Select(ir => new InspectionRoundDto(
                ir.Id, ir.PlantId, ir.Plant.Name,
                ir.InspectionDate, ir.InspectionMonth, ir.Status,
                ir.InspectedById,
                ir.EquipmentInspections.Count,
                ir.EquipmentInspections.Count(ei => ei.IsComplete),
                ir.CompletedAt))
            .ToListAsync(ct);
    }

    public async Task<List<FailedInspectionItemDto>> GetFailedItemsAsync(CancellationToken ct = default)
    {
        var failedEquipment = await context.EquipmentInspections
            .Where(ei => ei.InspectionRound.Status != InspectionStatus.Draft
                      && ei.InspectionRound.Status != InspectionStatus.Reviewed
                      && ei.Responses.Any(r => r.Response == false))
            .OrderByDescending(ei => ei.InspectionRound.InspectionDate)
            .Select(ei => new FailedInspectionItemDto(
                ei.InspectionRoundId,
                ei.Id,
                ei.EquipmentId,
                ei.Equipment.Identifier,
                ei.Equipment.EquipmentType.Name,
                ei.InspectionRound.Plant.Name,
                ei.Equipment.Section.Name,
                ei.InspectionRound.InspectionDate,
                ei.InspectionRound.InspectionMonth,
                ei.Responses
                    .Where(r => r.Response == false)
                    .Select(r => r.ChecklistItemTemplate.ItemName)
                    .ToList()))
            .ToListAsync(ct);

        return failedEquipment;
    }
}
