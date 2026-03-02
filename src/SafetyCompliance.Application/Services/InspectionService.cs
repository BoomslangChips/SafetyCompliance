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

    public async Task<InspectionRoundDto> StartInspectionRoundAsync(int plantId, string userId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var month = today.ToString("yyyy-MM");

        var existing = await context.InspectionRounds
            .AnyAsync(ir => ir.PlantId == plantId && ir.InspectionMonth == month, ct);

        if (existing)
            throw new InvalidOperationException($"An inspection round already exists for {month}");

        var round = new InspectionRound
        {
            PlantId = plantId,
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
            var checklistItems = await context.ChecklistItemTemplates
                .Where(c => c.EquipmentTypeId == ei.Equipment.EquipmentTypeId && c.IsActive)
                .ToListAsync(ct);

            foreach (var item in checklistItems)
            {
                context.InspectionResponses.Add(new InspectionResponse
                {
                    EquipmentInspectionId = ei.Id,
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

        round.Status = InspectionStatus.Completed;
        round.CompletedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
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

    public async Task<List<InspectionRoundDto>> GetActiveRoundsAsync(CancellationToken ct = default)
    {
        return await context.InspectionRounds
            .Where(ir => ir.Status == InspectionStatus.InProgress || ir.Status == InspectionStatus.Draft)
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
            .Where(ei => ei.InspectionRound.Status == InspectionStatus.InProgress
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
