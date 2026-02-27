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
                ir.Status, ir.InspectedBy.FirstName + " " + ir.InspectedBy.LastName,
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
            var inspection = new EquipmentInspection
            {
                InspectionRoundId = round.Id,
                EquipmentId = eq.Id
            };
            context.EquipmentInspections.Add(inspection);
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
        return await context.EquipmentInspections
            .Where(ei => ei.InspectionRoundId == roundId)
            .OrderBy(ei => ei.Equipment.Section.SortOrder)
            .ThenBy(ei => ei.Equipment.SortOrder)
            .Select(ei => new EquipmentInspectionDto(
                ei.Id, ei.InspectionRoundId, ei.EquipmentId,
                ei.Equipment.Identifier, ei.Equipment.Description ?? "",
                ei.Equipment.EquipmentType.Name,
                ei.Equipment.EquipmentSubType != null ? ei.Equipment.EquipmentSubType.Name : null,
                ei.Equipment.Size, ei.IsComplete, ei.Comments,
                ei.Responses
                    .OrderBy(r => r.ChecklistItemTemplate.SortOrder)
                    .Select(r => new InspectionResponseDto(
                        r.Id, r.ChecklistItemTemplateId, r.ChecklistItemTemplate.ItemName,
                        r.ChecklistItemTemplate.SortOrder, r.Response, r.Comment))
                    .ToList(),
                ei.Photos.Count))
            .ToListAsync(ct);
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
}
