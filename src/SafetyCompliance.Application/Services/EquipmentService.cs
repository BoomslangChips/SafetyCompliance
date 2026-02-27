using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class EquipmentService(ApplicationDbContext context) : IEquipmentService
{
    public async Task<List<EquipmentDto>> GetEquipmentBySectionAsync(int sectionId, CancellationToken ct = default)
    {
        return await context.Equipment
            .Where(e => e.SectionId == sectionId && e.IsActive)
            .OrderBy(e => e.SortOrder)
            .Select(e => new EquipmentDto(
                e.Id, e.SectionId, e.Section.Name, e.EquipmentTypeId, e.EquipmentType.Name,
                e.EquipmentSubTypeId, e.EquipmentSubType != null ? e.EquipmentSubType.Name : null,
                e.Identifier, e.Description, e.Size, e.SerialNumber, e.NextServiceDate,
                e.SortOrder, e.IsActive))
            .ToListAsync(ct);
    }

    public async Task<EquipmentDto?> GetEquipmentByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Equipment
            .Where(e => e.Id == id)
            .Select(e => new EquipmentDto(
                e.Id, e.SectionId, e.Section.Name, e.EquipmentTypeId, e.EquipmentType.Name,
                e.EquipmentSubTypeId, e.EquipmentSubType != null ? e.EquipmentSubType.Name : null,
                e.Identifier, e.Description, e.Size, e.SerialNumber, e.NextServiceDate,
                e.SortOrder, e.IsActive))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto, string userId, CancellationToken ct = default)
    {
        var equipment = new Equipment
        {
            SectionId = dto.SectionId,
            EquipmentTypeId = dto.EquipmentTypeId,
            EquipmentSubTypeId = dto.EquipmentSubTypeId,
            Identifier = dto.Identifier,
            Description = dto.Description,
            Size = dto.Size,
            SerialNumber = dto.SerialNumber,
            InstallDate = dto.InstallDate,
            NextServiceDate = dto.NextServiceDate,
            SortOrder = dto.SortOrder,
            CreatedById = userId
        };

        context.Equipment.Add(equipment);
        await context.SaveChangesAsync(ct);

        return (await GetEquipmentByIdAsync(equipment.Id, ct))!;
    }

    public async Task UpdateEquipmentAsync(EquipmentUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var equipment = await context.Equipment.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Equipment {dto.Id} not found");

        equipment.EquipmentTypeId = dto.EquipmentTypeId;
        equipment.EquipmentSubTypeId = dto.EquipmentSubTypeId;
        equipment.Identifier = dto.Identifier;
        equipment.Description = dto.Description;
        equipment.Size = dto.Size;
        equipment.SerialNumber = dto.SerialNumber;
        equipment.NextServiceDate = dto.NextServiceDate;
        equipment.SortOrder = dto.SortOrder;
        equipment.IsActive = dto.IsActive;
        equipment.ModifiedAt = DateTime.UtcNow;
        equipment.ModifiedById = userId;

        await context.SaveChangesAsync(ct);
    }

    public async Task<List<EquipmentTypeDto>> GetEquipmentTypesAsync(CancellationToken ct = default)
    {
        return await context.EquipmentTypes
            .Where(et => et.IsActive)
            .Select(et => new EquipmentTypeDto(et.Id, et.Name, et.Description, et.IsActive,
                et.ChecklistItemTemplates.Count(c => c.IsActive),
                et.SubTypes.Count(s => s.IsActive)))
            .ToListAsync(ct);
    }

    public async Task<EquipmentTypeDto?> GetEquipmentTypeByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.EquipmentTypes
            .Where(et => et.Id == id)
            .Select(et => new EquipmentTypeDto(et.Id, et.Name, et.Description, et.IsActive,
                et.ChecklistItemTemplates.Count(c => c.IsActive),
                et.SubTypes.Count(s => s.IsActive)))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EquipmentTypeDto> CreateEquipmentTypeAsync(EquipmentTypeCreateDto dto, CancellationToken ct = default)
    {
        var equipmentType = new EquipmentType
        {
            Name = dto.Name,
            Description = dto.Description
        };

        context.EquipmentTypes.Add(equipmentType);
        await context.SaveChangesAsync(ct);

        return new EquipmentTypeDto(equipmentType.Id, equipmentType.Name, equipmentType.Description, equipmentType.IsActive, 0, 0);
    }

    public async Task UpdateEquipmentTypeAsync(EquipmentTypeUpdateDto dto, CancellationToken ct = default)
    {
        var et = await context.EquipmentTypes.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"EquipmentType {dto.Id} not found");

        et.Name = dto.Name;
        et.Description = dto.Description;
        et.IsActive = dto.IsActive;

        await context.SaveChangesAsync(ct);
    }

    public async Task<List<EquipmentSubTypeDto>> GetSubTypesByTypeAsync(int equipmentTypeId, CancellationToken ct = default)
    {
        return await context.EquipmentSubTypes
            .Where(st => st.EquipmentTypeId == equipmentTypeId && st.IsActive)
            .Select(st => new EquipmentSubTypeDto(st.Id, st.EquipmentTypeId, st.Name, st.IsActive))
            .ToListAsync(ct);
    }

    public async Task<EquipmentSubTypeDto> CreateSubTypeAsync(EquipmentSubTypeCreateDto dto, CancellationToken ct = default)
    {
        var subType = new EquipmentSubType
        {
            EquipmentTypeId = dto.EquipmentTypeId,
            Name = dto.Name
        };

        context.EquipmentSubTypes.Add(subType);
        await context.SaveChangesAsync(ct);

        return new EquipmentSubTypeDto(subType.Id, subType.EquipmentTypeId, subType.Name, subType.IsActive);
    }

    public async Task<List<ChecklistItemTemplateDto>> GetChecklistTemplatesAsync(int equipmentTypeId, CancellationToken ct = default)
    {
        return await context.ChecklistItemTemplates
            .Where(c => c.EquipmentTypeId == equipmentTypeId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new ChecklistItemTemplateDto(c.Id, c.EquipmentTypeId, c.ItemName, c.Description, c.SortOrder, c.IsRequired, c.IsActive))
            .ToListAsync(ct);
    }

    public async Task<ChecklistItemTemplateDto> CreateChecklistTemplateAsync(ChecklistItemTemplateCreateDto dto, CancellationToken ct = default)
    {
        var template = new ChecklistItemTemplate
        {
            EquipmentTypeId = dto.EquipmentTypeId,
            ItemName = dto.ItemName,
            Description = dto.Description,
            SortOrder = dto.SortOrder,
            IsRequired = dto.IsRequired
        };

        context.ChecklistItemTemplates.Add(template);
        await context.SaveChangesAsync(ct);

        return new ChecklistItemTemplateDto(template.Id, template.EquipmentTypeId, template.ItemName, template.Description, template.SortOrder, template.IsRequired, template.IsActive);
    }

    public async Task UpdateChecklistTemplateAsync(ChecklistItemTemplateUpdateDto dto, CancellationToken ct = default)
    {
        var template = await context.ChecklistItemTemplates.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"ChecklistItemTemplate {dto.Id} not found");

        template.ItemName = dto.ItemName;
        template.Description = dto.Description;
        template.SortOrder = dto.SortOrder;
        template.IsRequired = dto.IsRequired;
        template.IsActive = dto.IsActive;

        await context.SaveChangesAsync(ct);
    }
}
