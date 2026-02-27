using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class SectionService(ApplicationDbContext context) : ISectionService
{
    public async Task<List<SectionDto>> GetSectionsByPlantAsync(int plantId, CancellationToken ct = default)
    {
        return await context.Sections
            .Where(s => s.PlantId == plantId && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .Select(s => new SectionDto(
                s.Id, s.PlantId, s.Plant.Name, s.Name, s.Description, s.SortOrder, s.IsActive,
                s.Equipment.Count(e => e.IsActive),
                s.PhotoBase64, s.PhotoFileName))
            .ToListAsync(ct);
    }

    public async Task<SectionDto?> GetSectionByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Sections
            .Where(s => s.Id == id)
            .Select(s => new SectionDto(
                s.Id, s.PlantId, s.Plant.Name, s.Name, s.Description, s.SortOrder, s.IsActive,
                s.Equipment.Count(e => e.IsActive),
                s.PhotoBase64, s.PhotoFileName))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SectionDto> CreateSectionAsync(SectionCreateDto dto, string userId, CancellationToken ct = default)
    {
        var section = new Section
        {
            PlantId = dto.PlantId,
            Name = dto.Name,
            Description = dto.Description,
            SortOrder = dto.SortOrder,
            PhotoBase64 = dto.PhotoBase64,
            PhotoFileName = dto.PhotoFileName,
            CreatedById = userId
        };

        context.Sections.Add(section);
        await context.SaveChangesAsync(ct);

        var plantName = await context.Plants.Where(p => p.Id == dto.PlantId).Select(p => p.Name).FirstAsync(ct);
        return new SectionDto(section.Id, section.PlantId, plantName, section.Name, section.Description, section.SortOrder, section.IsActive, 0, section.PhotoBase64, section.PhotoFileName);
    }

    public async Task UpdateSectionAsync(SectionUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var section = await context.Sections.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Section {dto.Id} not found");

        section.Name = dto.Name;
        section.Description = dto.Description;
        section.SortOrder = dto.SortOrder;
        section.IsActive = dto.IsActive;
        if (dto.PhotoBase64 is not null)
        {
            section.PhotoBase64 = dto.PhotoBase64;
            section.PhotoFileName = dto.PhotoFileName;
        }
        section.ModifiedAt = DateTime.UtcNow;
        section.ModifiedById = userId;

        await context.SaveChangesAsync(ct);
    }
}
