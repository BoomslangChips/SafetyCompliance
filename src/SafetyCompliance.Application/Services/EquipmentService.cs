using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class EquipmentService(ApplicationDbContext context) : IEquipmentService
{
    // ── Equipment ─────────────────────────────────────────────────────────────

    public async Task<List<EquipmentDto>> GetEquipmentBySectionAsync(int sectionId, bool includeInactive = false, CancellationToken ct = default)
    {
        return await context.Equipment
            .Where(e => e.SectionId == sectionId && (includeInactive || e.IsActive))
            .OrderBy(e => e.SortOrder)
            .Select(e => new EquipmentDto(
                e.Id, e.SectionId, e.Section != null ? e.Section.Name : null, e.EquipmentTypeId, e.EquipmentType.Name,
                e.EquipmentSubTypeId, e.EquipmentSubType != null ? e.EquipmentSubType.Name : null,
                e.Identifier, e.Description, e.Size, e.SerialNumber, e.NextServiceDate,
                e.SortOrder, e.IsActive, e.Status))
            .ToListAsync(ct);
    }

    public async Task<EquipmentDto?> GetEquipmentByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Equipment
            .Where(e => e.Id == id)
            .Select(e => new EquipmentDto(
                e.Id, e.SectionId, e.Section != null ? e.Section.Name : null, e.EquipmentTypeId, e.EquipmentType.Name,
                e.EquipmentSubTypeId, e.EquipmentSubType != null ? e.EquipmentSubType.Name : null,
                e.Identifier, e.Description, e.Size, e.SerialNumber, e.NextServiceDate,
                e.SortOrder, e.IsActive, e.Status))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto, string userId, CancellationToken ct = default)
    {
        var equipment = new Equipment
        {
            SectionId          = dto.SectionId,
            EquipmentTypeId    = dto.EquipmentTypeId,
            EquipmentSubTypeId = dto.EquipmentSubTypeId,
            Identifier         = dto.Identifier,
            Description        = dto.Description,
            Size               = dto.Size,
            SerialNumber       = dto.SerialNumber,
            InstallDate        = dto.InstallDate,
            NextServiceDate    = dto.NextServiceDate,
            SortOrder          = dto.SortOrder,
            CreatedById        = userId
        };

        context.Equipment.Add(equipment);
        await context.SaveChangesAsync(ct);
        return (await GetEquipmentByIdAsync(equipment.Id, ct))!;
    }

    public async Task UpdateEquipmentAsync(EquipmentUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var equipment = await context.Equipment.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Equipment {dto.Id} not found");

        equipment.EquipmentTypeId    = dto.EquipmentTypeId;
        equipment.EquipmentSubTypeId = dto.EquipmentSubTypeId;
        equipment.Identifier         = dto.Identifier;
        equipment.Description        = dto.Description;
        equipment.Size               = dto.Size;
        equipment.SerialNumber       = dto.SerialNumber;
        equipment.NextServiceDate    = dto.NextServiceDate;
        equipment.SortOrder          = dto.SortOrder;
        equipment.IsActive           = dto.IsActive;
        equipment.ModifiedAt         = DateTime.UtcNow;
        equipment.ModifiedById       = userId;

        await context.SaveChangesAsync(ct);
    }

    // ── Equipment Types ───────────────────────────────────────────────────────

    public async Task<List<EquipmentTypeDto>> GetEquipmentTypesAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        return await context.EquipmentTypes
            .Where(et => includeInactive || et.IsActive)
            .Select(et => new EquipmentTypeDto(et.Id, et.Name, et.Description, et.IsActive,
                et.ChecklistItemTemplates.Count(c => c.IsActive),
                et.SubTypes.Count(s => s.IsActive),
                et.EquipmentChecks.Count(ec => ec.IsActive)))
            .ToListAsync(ct);
    }

    public async Task<EquipmentTypeDto?> GetEquipmentTypeByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.EquipmentTypes
            .Where(et => et.Id == id)
            .Select(et => new EquipmentTypeDto(et.Id, et.Name, et.Description, et.IsActive,
                et.ChecklistItemTemplates.Count(c => c.IsActive),
                et.SubTypes.Count(s => s.IsActive),
                et.EquipmentChecks.Count(ec => ec.IsActive)))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<EquipmentTypeDto> CreateEquipmentTypeAsync(EquipmentTypeCreateDto dto, CancellationToken ct = default)
    {
        var equipmentType = new EquipmentType { Name = dto.Name, Description = dto.Description };
        context.EquipmentTypes.Add(equipmentType);
        await context.SaveChangesAsync(ct);
        return new EquipmentTypeDto(equipmentType.Id, equipmentType.Name, equipmentType.Description, equipmentType.IsActive, 0, 0, 0);
    }

    public async Task UpdateEquipmentTypeAsync(EquipmentTypeUpdateDto dto, CancellationToken ct = default)
    {
        var et = await context.EquipmentTypes.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"EquipmentType {dto.Id} not found");
        et.Name        = dto.Name;
        et.Description = dto.Description;
        et.IsActive    = dto.IsActive;
        await context.SaveChangesAsync(ct);
    }

    public async Task<DeleteResult> DeleteOrDeactivateEquipmentTypeAsync(int id, CancellationToken ct = default)
    {
        var hasEquipment = await context.Equipment.AnyAsync(e => e.EquipmentTypeId == id, ct);
        if (!hasEquipment)
        {
            var templates = await context.ChecklistItemTemplates.Where(t => t.EquipmentTypeId == id).ToListAsync(ct);
            context.ChecklistItemTemplates.RemoveRange(templates);
            var checks = await context.EquipmentChecks.Where(c => c.EquipmentTypeId == id).ToListAsync(ct);
            context.EquipmentChecks.RemoveRange(checks);
            var subTypes = await context.EquipmentSubTypes.Where(s => s.EquipmentTypeId == id).ToListAsync(ct);
            context.EquipmentSubTypes.RemoveRange(subTypes);
            var et = await context.EquipmentTypes.FindAsync([id], ct);
            if (et != null) context.EquipmentTypes.Remove(et);
            await context.SaveChangesAsync(ct);
            return new DeleteResult(DeleteOutcome.Deleted, "Equipment type permanently deleted.");
        }

        var inActiveRound = await context.EquipmentInspections
            .AnyAsync(ei => ei.Equipment.EquipmentTypeId == id
                         && (ei.InspectionRound.Status == InspectionStatus.InProgress
                             || ei.InspectionRound.Status == InspectionStatus.Draft), ct);

        if (inActiveRound)
            return new DeleteResult(DeleteOutcome.Blocked,
                "Cannot disable: this equipment type is currently part of an active inspection round.");

        var type = await context.EquipmentTypes.FindAsync([id], ct);
        if (type != null) { type.IsActive = false; await context.SaveChangesAsync(ct); }
        return new DeleteResult(DeleteOutcome.Deactivated,
            "Equipment type disabled. Existing equipment is unaffected but it will no longer appear in selection lists.");
    }

    // ── Sub-Types ─────────────────────────────────────────────────────────────

    public async Task<List<EquipmentSubTypeDto>> GetSubTypesByTypeAsync(int equipmentTypeId, bool includeInactive = false, CancellationToken ct = default)
    {
        return await context.EquipmentSubTypes
            .Where(st => st.EquipmentTypeId == equipmentTypeId && (includeInactive || st.IsActive))
            .Select(st => new EquipmentSubTypeDto(st.Id, st.EquipmentTypeId, st.Name, st.IsActive))
            .ToListAsync(ct);
    }

    public async Task<EquipmentSubTypeDto> CreateSubTypeAsync(EquipmentSubTypeCreateDto dto, CancellationToken ct = default)
    {
        var subType = new EquipmentSubType { EquipmentTypeId = dto.EquipmentTypeId, Name = dto.Name };
        context.EquipmentSubTypes.Add(subType);
        await context.SaveChangesAsync(ct);
        return new EquipmentSubTypeDto(subType.Id, subType.EquipmentTypeId, subType.Name, subType.IsActive);
    }

    public async Task UpdateSubTypeAsync(EquipmentSubTypeUpdateDto dto, CancellationToken ct = default)
    {
        var st = await context.EquipmentSubTypes.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"SubType {dto.Id} not found");
        st.Name     = dto.Name;
        st.IsActive = dto.IsActive;
        await context.SaveChangesAsync(ct);
    }

    public async Task<DeleteResult> DeleteOrDeactivateSubTypeAsync(int id, CancellationToken ct = default)
    {
        var hasEquipment  = await context.Equipment.AnyAsync(e => e.EquipmentSubTypeId == id, ct);
        var hasTemplates  = await context.ChecklistItemTemplates.AnyAsync(t => t.EquipmentSubTypeId == id, ct);
        var hasChecks     = await context.EquipmentChecks.AnyAsync(c => c.EquipmentSubTypeId == id, ct);

        if (!hasEquipment && !hasTemplates && !hasChecks)
        {
            var st = await context.EquipmentSubTypes.FindAsync([id], ct);
            if (st != null) context.EquipmentSubTypes.Remove(st);
            await context.SaveChangesAsync(ct);
            return new DeleteResult(DeleteOutcome.Deleted, "Sub-type permanently deleted.");
        }

        var inActiveRound = await context.EquipmentInspections
            .AnyAsync(ei => ei.Equipment.EquipmentSubTypeId == id
                         && (ei.InspectionRound.Status == InspectionStatus.InProgress
                             || ei.InspectionRound.Status == InspectionStatus.Draft), ct);

        if (inActiveRound)
            return new DeleteResult(DeleteOutcome.Blocked,
                "Cannot disable: this sub-type is part of an active inspection round.");

        var st2 = await context.EquipmentSubTypes.FindAsync([id], ct);
        if (st2 != null) { st2.IsActive = false; await context.SaveChangesAsync(ct); }
        return new DeleteResult(DeleteOutcome.Deactivated,
            "Sub-type disabled. It won't appear in selection lists; existing equipment is unaffected.");
    }

    // ── Checklist Templates ───────────────────────────────────────────────────

    public async Task<List<ChecklistItemTemplateDto>> GetChecklistTemplatesAsync(int equipmentTypeId, bool includeInactive = false, CancellationToken ct = default)
    {
        return await context.ChecklistItemTemplates
            .Where(c => c.EquipmentTypeId == equipmentTypeId && (includeInactive || c.IsActive))
            .OrderBy(c => c.EquipmentSubTypeId == null ? 0 : 1)
            .ThenBy(c => c.SortOrder)
            .Select(c => new ChecklistItemTemplateDto(
                c.Id, c.EquipmentTypeId,
                c.EquipmentSubTypeId,
                c.EquipmentSubType != null ? c.EquipmentSubType.Name : null,
                c.ItemName, c.Description, c.SortOrder, c.IsRequired, c.IsActive))
            .ToListAsync(ct);
    }

    public async Task<ChecklistItemTemplateDto> CreateChecklistTemplateAsync(ChecklistItemTemplateCreateDto dto, CancellationToken ct = default)
    {
        var template = new ChecklistItemTemplate
        {
            EquipmentTypeId    = dto.EquipmentTypeId,
            EquipmentSubTypeId = dto.EquipmentSubTypeId,
            ItemName           = dto.ItemName,
            Description        = dto.Description,
            SortOrder          = dto.SortOrder,
            IsRequired         = dto.IsRequired
        };

        context.ChecklistItemTemplates.Add(template);
        await context.SaveChangesAsync(ct);

        var subTypeName = dto.EquipmentSubTypeId.HasValue
            ? await context.EquipmentSubTypes
                .Where(s => s.Id == dto.EquipmentSubTypeId.Value)
                .Select(s => s.Name)
                .FirstOrDefaultAsync(ct)
            : null;

        return new ChecklistItemTemplateDto(
            template.Id, template.EquipmentTypeId,
            template.EquipmentSubTypeId, subTypeName,
            template.ItemName, template.Description,
            template.SortOrder, template.IsRequired, template.IsActive);
    }

    public async Task UpdateChecklistTemplateAsync(ChecklistItemTemplateUpdateDto dto, CancellationToken ct = default)
    {
        var template = await context.ChecklistItemTemplates.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"ChecklistItemTemplate {dto.Id} not found");
        template.ItemName    = dto.ItemName;
        template.Description = dto.Description;
        template.SortOrder   = dto.SortOrder;
        template.IsRequired  = dto.IsRequired;
        template.IsActive    = dto.IsActive;
        await context.SaveChangesAsync(ct);
    }

    public async Task<DeleteResult> DeleteOrDeactivateChecklistTemplateAsync(int id, CancellationToken ct = default)
    {
        var hasResponses = await context.InspectionResponses.AnyAsync(r => r.ChecklistItemTemplateId == id, ct);
        if (!hasResponses)
        {
            var t = await context.ChecklistItemTemplates.FindAsync([id], ct);
            if (t != null) context.ChecklistItemTemplates.Remove(t);
            await context.SaveChangesAsync(ct);
            return new DeleteResult(DeleteOutcome.Deleted, "Checklist item permanently deleted.");
        }

        var inActiveRound = await context.InspectionResponses
            .AnyAsync(r => r.ChecklistItemTemplateId == id
                        && (r.EquipmentInspection.InspectionRound.Status == InspectionStatus.InProgress
                            || r.EquipmentInspection.InspectionRound.Status == InspectionStatus.Draft), ct);

        if (inActiveRound)
            return new DeleteResult(DeleteOutcome.Blocked,
                "Cannot modify: this checklist item is part of an active inspection in progress.");

        var t2 = await context.ChecklistItemTemplates.FindAsync([id], ct);
        if (t2 != null) { t2.IsActive = false; await context.SaveChangesAsync(ct); }
        return new DeleteResult(DeleteOutcome.Deactivated,
            "Checklist item disabled. It won't appear in future inspections; historical data is preserved.");
    }

    // ── Inventory ─────────────────────────────────────────────────────────────

    public async Task<List<InventoryEquipmentDto>> GetInventoryAsync(
        int? equipmentTypeId = null, EquipmentStatus? status = null,
        string? complianceFilter = null, string? searchTerm = null,
        bool? isAssigned = null, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        var query = context.Equipment.Where(e => e.IsActive);

        if (equipmentTypeId.HasValue)
            query = query.Where(e => e.EquipmentTypeId == equipmentTypeId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (isAssigned == true)
            query = query.Where(e => e.SectionId != null);
        else if (isAssigned == false)
            query = query.Where(e => e.SectionId == null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(e =>
                e.Identifier.ToLower().Contains(term) ||
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(term)) ||
                (e.Description != null && e.Description.ToLower().Contains(term)));
        }

        var items = await query
            .OrderBy(e => e.Identifier)
            .Select(e => new InventoryEquipmentDto(
                e.Id, e.Identifier, e.Description, e.Size, e.SerialNumber, e.IsActive,
                e.SectionId,
                e.Section != null ? e.Section.Name : null,
                e.Section != null ? (int?)e.Section.PlantId : null,
                e.Section != null ? e.Section.Plant.Name : null,
                e.Section != null ? (int?)e.Section.Plant.CompanyId : null,
                e.Section != null ? e.Section.Plant.Company.Name : null,
                e.EquipmentTypeId, e.EquipmentType.Name,
                e.EquipmentSubTypeId, e.EquipmentSubType != null ? e.EquipmentSubType.Name : null,
                e.Status,
                e.CheckRecords.Count(),
                e.CheckRecords.Count(cr => cr.ExpiryDate != null && cr.ExpiryDate < today),
                e.CheckRecords.Count(cr => cr.ExpiryDate != null && cr.ExpiryDate >= today && cr.ExpiryDate <= dueSoonDate)))
            .ToListAsync(ct);

        // Post-filter compliance status (cannot be done efficiently in the EF query)
        if (complianceFilter == "Overdue")
            items = items.Where(i => i.OverdueChecks > 0).ToList();
        else if (complianceFilter == "DueSoon")
            items = items.Where(i => i.DueSoonChecks > 0 && i.OverdueChecks == 0).ToList();
        else if (complianceFilter == "OK")
            items = items.Where(i => i.OverdueChecks == 0 && i.DueSoonChecks == 0).ToList();

        return items;
    }

    public async Task<InventoryStatsDto> GetInventoryStatsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        var equipment = await context.Equipment
            .Where(e => e.IsActive)
            .Select(e => new
            {
                e.Status,
                IsAssigned = e.SectionId != null,
                OverdueCount = e.CheckRecords.Count(cr => cr.ExpiryDate != null && cr.ExpiryDate < today),
                DueSoonCount = e.CheckRecords.Count(cr => cr.ExpiryDate != null && cr.ExpiryDate >= today && cr.ExpiryDate <= dueSoonDate)
            })
            .ToListAsync(ct);

        return new InventoryStatsDto(
            TotalEquipment: equipment.Count,
            AssignedCount: equipment.Count(e => e.IsAssigned),
            AvailableCount: equipment.Count(e => !e.IsAssigned),
            InServiceCount: equipment.Count(e => e.Status == EquipmentStatus.InService),
            DamagedCount: equipment.Count(e => e.Status == EquipmentStatus.Damaged),
            MissingCount: equipment.Count(e => e.Status == EquipmentStatus.Missing),
            OutOfServiceCount: equipment.Count(e => e.Status == EquipmentStatus.OutOfService),
            NeedsReplacementCount: equipment.Count(e => e.Status == EquipmentStatus.NeedsReplacement),
            RetiredCount: equipment.Count(e => e.Status == EquipmentStatus.Retired),
            OverdueComplianceCount: equipment.Count(e => e.OverdueCount > 0),
            DueSoonComplianceCount: equipment.Count(e => e.DueSoonCount > 0 && e.OverdueCount == 0));
    }

    public async Task<EquipmentDetailDto?> GetEquipmentDetailAsync(int equipmentId, CancellationToken ct = default)
    {
        var eq = await context.Equipment
            .Include(e => e.Section).ThenInclude(s => s!.Plant).ThenInclude(p => p.Company)
            .Include(e => e.EquipmentType)
            .Include(e => e.EquipmentSubType)
            .Include(e => e.CheckRecords).ThenInclude(cr => cr.EquipmentCheck)
            .FirstOrDefaultAsync(e => e.Id == equipmentId, ct);

        if (eq == null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get all applicable checks for this type/subtype
        var checks = await context.EquipmentChecks
            .Where(c => c.EquipmentTypeId == eq.EquipmentTypeId && c.IsActive
                && (c.EquipmentSubTypeId == null || c.EquipmentSubTypeId == eq.EquipmentSubTypeId))
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        var complianceChecks = checks.Select(c =>
        {
            var record = eq.CheckRecords.FirstOrDefault(r => r.EquipmentCheckId == c.Id);
            string status = "Not Set";
            if (record != null)
            {
                status = record.ExpiryDate == null ? "OK"
                    : record.ExpiryDate < today ? "Overdue"
                    : record.ExpiryDate < today.AddDays(30) ? "Due Soon"
                    : "OK";
            }

            return new EquipmentCheckWithRecordDto(
                c.Id, c.Name, c.Description, c.IntervalMonths, c.IsRequired,
                record?.Id, record?.DateValue, record?.ExpiryDate,
                record?.Notes, status);
        }).ToList();

        return new EquipmentDetailDto(
            eq.Id, eq.Identifier, eq.Description, eq.Size,
            eq.SerialNumber, eq.Status, eq.IsActive,
            eq.EquipmentTypeId, eq.EquipmentType.Name,
            eq.EquipmentSubTypeId, eq.EquipmentSubType?.Name,
            eq.SectionId, eq.Section?.Name,
            eq.Section != null ? eq.Section.PlantId : null,
            eq.Section?.Plant?.Name,
            eq.Section?.Plant != null ? eq.Section.Plant.CompanyId : null,
            eq.Section?.Plant?.Company?.Name,
            eq.InstallDate, eq.LastServiceDate, eq.NextServiceDate,
            eq.CreatedAt, eq.ModifiedAt,
            complianceChecks);
    }

    public async Task<List<EquipmentDto>> CreateInventoryEquipmentBulkAsync(EquipmentInventoryCreateDto dto, string userId, CancellationToken ct = default)
    {
        var maxSort = await context.Equipment
            .Where(e => e.EquipmentTypeId == dto.EquipmentTypeId)
            .MaxAsync(e => (int?)e.SortOrder, ct) ?? 0;

        // Find the highest existing number for this prefix
        var prefix = dto.IdentifierPrefix.Trim();
        var existingIdentifiers = await context.Equipment
            .Where(e => e.Identifier.StartsWith(prefix + "-"))
            .Select(e => e.Identifier)
            .ToListAsync(ct);

        int nextNumber = 1;
        foreach (var id in existingIdentifiers)
        {
            var suffix = id[(prefix.Length + 1)..];
            if (int.TryParse(suffix, out var num) && num >= nextNumber)
                nextNumber = num + 1;
        }

        var created = new List<Equipment>();
        for (int i = 0; i < dto.Quantity; i++)
        {
            var identifier = dto.Quantity == 1 && prefix.Contains('-')
                ? prefix  // Allow explicit single identifier like "FE-CUSTOM"
                : $"{prefix}-{(nextNumber + i):D3}";

            var equipment = new Equipment
            {
                SectionId          = null,
                EquipmentTypeId    = dto.EquipmentTypeId,
                EquipmentSubTypeId = dto.EquipmentSubTypeId,
                Identifier         = identifier,
                Description        = dto.Description,
                Size               = dto.Size,
                SerialNumber       = dto.Quantity == 1 ? dto.SerialNumber : null,
                Status             = dto.Status,
                SortOrder          = maxSort + i + 1,
                CreatedById        = userId
            };
            context.Equipment.Add(equipment);
            created.Add(equipment);
        }

        await context.SaveChangesAsync(ct);

        // Re-fetch with full type names
        var ids = created.Select(e => e.Id).ToList();
        return await context.Equipment
            .Where(e => ids.Contains(e.Id))
            .Select(e => new EquipmentDto(
                e.Id, e.SectionId, null, e.EquipmentTypeId, e.EquipmentType.Name,
                e.EquipmentSubTypeId, e.EquipmentSubType != null ? e.EquipmentSubType.Name : null,
                e.Identifier, e.Description, e.Size, e.SerialNumber,
                e.NextServiceDate, e.SortOrder, e.IsActive, e.Status))
            .ToListAsync(ct);
    }

    public async Task AssignEquipmentAsync(AssignEquipmentDto dto, string userId, CancellationToken ct = default)
    {
        var equipment = await context.Equipment.FindAsync([dto.EquipmentId], ct)
            ?? throw new InvalidOperationException($"Equipment {dto.EquipmentId} not found");

        equipment.SectionId    = dto.SectionId;
        equipment.ModifiedAt   = DateTime.UtcNow;
        equipment.ModifiedById = userId;
        await context.SaveChangesAsync(ct);
    }

    public async Task UnassignEquipmentAsync(UnassignEquipmentDto dto, string userId, CancellationToken ct = default)
    {
        var equipment = await context.Equipment.FindAsync([dto.EquipmentId], ct)
            ?? throw new InvalidOperationException($"Equipment {dto.EquipmentId} not found");

        equipment.SectionId    = null;
        equipment.ModifiedAt   = DateTime.UtcNow;
        equipment.ModifiedById = userId;
        await context.SaveChangesAsync(ct);
    }

    public async Task BulkAssignEquipmentAsync(List<int> equipmentIds, int sectionId, string userId, CancellationToken ct = default)
    {
        var items = await context.Equipment
            .Where(e => equipmentIds.Contains(e.Id))
            .ToListAsync(ct);

        foreach (var item in items)
        {
            item.SectionId    = sectionId;
            item.ModifiedAt   = DateTime.UtcNow;
            item.ModifiedById = userId;
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateEquipmentStatusAsync(EquipmentStatusUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var equipment = await context.Equipment.FindAsync([dto.EquipmentId], ct)
            ?? throw new InvalidOperationException($"Equipment {dto.EquipmentId} not found");

        equipment.Status       = dto.Status;
        equipment.ModifiedAt   = DateTime.UtcNow;
        equipment.ModifiedById = userId;
        await context.SaveChangesAsync(ct);
    }

    // ── Equipment Check Templates ─────────────────────────────────────────────

    public async Task<List<EquipmentCheckDto>> GetEquipmentChecksAsync(int equipmentTypeId, bool includeInactive = false, CancellationToken ct = default)
    {
        return await context.EquipmentChecks
            .Where(c => c.EquipmentTypeId == equipmentTypeId && (includeInactive || c.IsActive))
            .OrderBy(c => c.EquipmentSubTypeId == null ? 0 : 1)
            .ThenBy(c => c.SortOrder)
            .Select(c => new EquipmentCheckDto(
                c.Id, c.EquipmentTypeId, c.EquipmentSubTypeId,
                c.EquipmentSubType != null ? c.EquipmentSubType.Name : null,
                c.Name, c.Description, c.IntervalMonths, c.IsRequired,
                c.SortOrder, c.IsActive))
            .ToListAsync(ct);
    }

    public async Task<EquipmentCheckDto> CreateEquipmentCheckAsync(EquipmentCheckCreateDto dto, CancellationToken ct = default)
    {
        var check = new EquipmentCheck
        {
            EquipmentTypeId    = dto.EquipmentTypeId,
            EquipmentSubTypeId = dto.EquipmentSubTypeId,
            Name               = dto.Name,
            Description        = dto.Description,
            IntervalMonths     = dto.IntervalMonths,
            IsRequired         = dto.IsRequired,
            SortOrder          = dto.SortOrder
        };

        context.EquipmentChecks.Add(check);
        await context.SaveChangesAsync(ct);

        var subTypeName = dto.EquipmentSubTypeId.HasValue
            ? await context.EquipmentSubTypes
                .Where(s => s.Id == dto.EquipmentSubTypeId.Value)
                .Select(s => s.Name)
                .FirstOrDefaultAsync(ct)
            : null;

        return new EquipmentCheckDto(
            check.Id, check.EquipmentTypeId, check.EquipmentSubTypeId, subTypeName,
            check.Name, check.Description, check.IntervalMonths, check.IsRequired,
            check.SortOrder, check.IsActive);
    }

    public async Task UpdateEquipmentCheckAsync(EquipmentCheckUpdateDto dto, CancellationToken ct = default)
    {
        var check = await context.EquipmentChecks.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"EquipmentCheck {dto.Id} not found");
        check.Name           = dto.Name;
        check.Description    = dto.Description;
        check.IntervalMonths = dto.IntervalMonths;
        check.IsRequired     = dto.IsRequired;
        check.SortOrder      = dto.SortOrder;
        check.IsActive       = dto.IsActive;
        await context.SaveChangesAsync(ct);
    }

    public async Task<DeleteResult> DeleteOrDeactivateEquipmentCheckAsync(int id, CancellationToken ct = default)
    {
        var hasRecords = await context.EquipmentCheckRecords.AnyAsync(r => r.EquipmentCheckId == id, ct);
        if (!hasRecords)
        {
            var c = await context.EquipmentChecks.FindAsync([id], ct);
            if (c != null) context.EquipmentChecks.Remove(c);
            await context.SaveChangesAsync(ct);
            return new DeleteResult(DeleteOutcome.Deleted, "Compliance check permanently deleted.");
        }

        var c2 = await context.EquipmentChecks.FindAsync([id], ct);
        if (c2 != null) { c2.IsActive = false; await context.SaveChangesAsync(ct); }
        return new DeleteResult(DeleteOutcome.Deactivated,
            "Compliance check disabled. Historical records are preserved.");
    }

    public async Task<List<EquipmentCheckDto>> GetApplicableChecksAsync(
        int equipmentTypeId, int? equipmentSubTypeId = null, CancellationToken ct = default)
    {
        return await context.EquipmentChecks
            .Where(c => c.EquipmentTypeId == equipmentTypeId && c.IsActive
                && (c.EquipmentSubTypeId == null || c.EquipmentSubTypeId == equipmentSubTypeId))
            .OrderBy(c => c.SortOrder)
            .Select(c => new EquipmentCheckDto(
                c.Id, c.EquipmentTypeId, c.EquipmentSubTypeId,
                c.EquipmentSubType != null ? c.EquipmentSubType.Name : null,
                c.Name, c.Description, c.IntervalMonths, c.IsRequired,
                c.SortOrder, c.IsActive))
            .ToListAsync(ct);
    }

    // ── Check Records ─────────────────────────────────────────────────────────

    public async Task<List<EquipmentCheckRecordDto>> GetCheckRecordsAsync(int equipmentId, CancellationToken ct = default)
    {
        var records = await context.EquipmentCheckRecords
            .Where(r => r.EquipmentId == equipmentId)
            .Select(r => new EquipmentCheckRecordDto(
                r.Id, r.EquipmentId, r.EquipmentCheckId, r.EquipmentCheck.Name,
                r.EquipmentCheck.IntervalMonths, r.DateValue, r.ExpiryDate,
                r.Notes,
                r.ExpiryDate == null ? "OK"
                    : r.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow) ? "Overdue"
                    : r.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)) ? "Due Soon"
                    : "OK"))
            .ToListAsync(ct);

        return records;
    }

    public async Task<EquipmentCheckRecordDto> UpsertCheckRecordAsync(EquipmentCheckRecordUpsertDto dto, string userId, CancellationToken ct = default)
    {
        var check = await context.EquipmentChecks.FindAsync([dto.EquipmentCheckId], ct)
            ?? throw new InvalidOperationException($"EquipmentCheck {dto.EquipmentCheckId} not found");

        var expiryDate = check.IntervalMonths.HasValue
            ? dto.DateValue.AddMonths(check.IntervalMonths.Value)
            : (DateOnly?)null;

        var existing = await context.EquipmentCheckRecords
            .FirstOrDefaultAsync(r => r.EquipmentId == dto.EquipmentId && r.EquipmentCheckId == dto.EquipmentCheckId, ct);

        if (existing != null)
        {
            existing.DateValue    = dto.DateValue;
            existing.ExpiryDate   = expiryDate;
            existing.Notes        = dto.Notes;
            existing.ModifiedAt   = DateTime.UtcNow;
            existing.ModifiedById = userId;
        }
        else
        {
            existing = new EquipmentCheckRecord
            {
                EquipmentId      = dto.EquipmentId,
                EquipmentCheckId = dto.EquipmentCheckId,
                DateValue        = dto.DateValue,
                ExpiryDate       = expiryDate,
                Notes            = dto.Notes,
                CreatedById      = userId
            };
            context.EquipmentCheckRecords.Add(existing);
        }

        await context.SaveChangesAsync(ct);

        var complianceStatus = expiryDate == null ? "OK"
            : expiryDate < DateOnly.FromDateTime(DateTime.UtcNow) ? "Overdue"
            : expiryDate < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)) ? "Due Soon"
            : "OK";

        return new EquipmentCheckRecordDto(
            existing.Id, existing.EquipmentId, existing.EquipmentCheckId, check.Name,
            check.IntervalMonths, existing.DateValue, existing.ExpiryDate,
            existing.Notes, complianceStatus);
    }

    public async Task BulkUpsertCheckRecordsAsync(
        int equipmentId, List<EquipmentCheckRecordUpsertDto> records, string userId, CancellationToken ct = default)
    {
        foreach (var dto in records)
        {
            await UpsertCheckRecordAsync(
                new EquipmentCheckRecordUpsertDto(equipmentId, dto.EquipmentCheckId, dto.DateValue, dto.Notes),
                userId, ct);
        }
    }
}
