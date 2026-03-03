using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class NoteService(ApplicationDbContext context) : INoteService
{
    public async Task<List<NoteDto>> GetNotesAsync(
        int?         equipmentId = null,
        int?         plantId     = null,
        int?         companyId   = null,
        NoteCategory? category   = null,
        CancellationToken ct     = default)
    {
        var query = context.Notes.AsQueryable();

        if (equipmentId.HasValue)
            query = query.Where(n => n.EquipmentId == equipmentId.Value);

        if (plantId.HasValue)
            query = query.Where(n =>
                n.PlantId == plantId.Value ||
                (n.Equipment != null && n.Equipment.Section.PlantId == plantId.Value));

        if (companyId.HasValue)
            query = query.Where(n =>
                n.CompanyId == companyId.Value ||
                (n.Plant != null && n.Plant.CompanyId == companyId.Value) ||
                (n.Equipment != null && n.Equipment.Section.Plant.CompanyId == companyId.Value));

        if (category.HasValue)
            query = query.Where(n => n.Category == category.Value);

        return await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.Priority)
            .ThenByDescending(n => n.CreatedAt)
            .Select(n => new NoteDto(
                n.Id,
                n.Title,
                n.Content,
                n.Category,
                n.Priority,
                n.IsPinned,
                n.EquipmentId,
                n.Equipment != null ? n.Equipment.Identifier : null,
                n.Equipment != null ? n.Equipment.EquipmentType.Name : null,
                n.Equipment != null && n.Equipment.EquipmentSubType != null ? n.Equipment.EquipmentSubType.Name : null,
                n.CompanyId,
                n.Company != null ? n.Company.Name : null,
                n.PlantId,
                n.PlantId != null && n.Plant != null
                    ? n.Plant.Name
                    : (n.Equipment != null ? n.Equipment.Section.Plant.Name : null),
                n.Equipment != null ? n.Equipment.Section.Name : null,
                n.CreatedById,
                n.CreatedAt,
                n.ModifiedAt))
            .ToListAsync(ct);
    }

    public async Task<NoteDto?> GetNoteByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Notes
            .Where(n => n.Id == id)
            .Select(n => new NoteDto(
                n.Id,
                n.Title,
                n.Content,
                n.Category,
                n.Priority,
                n.IsPinned,
                n.EquipmentId,
                n.Equipment != null ? n.Equipment.Identifier : null,
                n.Equipment != null ? n.Equipment.EquipmentType.Name : null,
                n.Equipment != null && n.Equipment.EquipmentSubType != null ? n.Equipment.EquipmentSubType.Name : null,
                n.CompanyId,
                n.Company != null ? n.Company.Name : null,
                n.PlantId,
                n.PlantId != null && n.Plant != null
                    ? n.Plant.Name
                    : (n.Equipment != null ? n.Equipment.Section.Plant.Name : null),
                n.Equipment != null ? n.Equipment.Section.Name : null,
                n.CreatedById,
                n.CreatedAt,
                n.ModifiedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<NoteDto> CreateNoteAsync(NoteCreateDto dto, string userId, CancellationToken ct = default)
    {
        var note = new Note
        {
            Title       = dto.Title,
            Content     = dto.Content,
            Category    = dto.Category,
            Priority    = dto.Priority,
            IsPinned    = dto.IsPinned,
            EquipmentId = dto.EquipmentId,
            CompanyId   = dto.CompanyId,
            PlantId     = dto.PlantId,
            CreatedById = userId
        };

        context.Notes.Add(note);
        await context.SaveChangesAsync(ct);
        return (await GetNoteByIdAsync(note.Id, ct))!;
    }

    public async Task UpdateNoteAsync(NoteUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var note = await context.Notes.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Note {dto.Id} not found");

        note.Title      = dto.Title;
        note.Content    = dto.Content;
        note.Category   = dto.Category;
        note.Priority   = dto.Priority;
        note.IsPinned   = dto.IsPinned;
        note.ModifiedById = userId;
        note.ModifiedAt   = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task TogglePinnedAsync(int id, string userId, CancellationToken ct = default)
    {
        var note = await context.Notes.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Note {id} not found");

        note.IsPinned     = !note.IsPinned;
        note.ModifiedById = userId;
        note.ModifiedAt   = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteNoteAsync(int id, CancellationToken ct = default)
    {
        var note = await context.Notes.FindAsync([id], ct);
        if (note is not null)
        {
            context.Notes.Remove(note);
            await context.SaveChangesAsync(ct);
        }
    }
}
