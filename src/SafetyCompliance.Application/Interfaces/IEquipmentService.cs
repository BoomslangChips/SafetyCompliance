using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.Interfaces;

public interface IEquipmentService
{
    Task<List<EquipmentDto>> GetEquipmentBySectionAsync(int sectionId, bool includeInactive = false, CancellationToken ct = default);
    Task<EquipmentDto?> GetEquipmentByIdAsync(int id, CancellationToken ct = default);
    Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateEquipmentAsync(EquipmentUpdateDto dto, string userId, CancellationToken ct = default);

    Task<List<EquipmentTypeDto>> GetEquipmentTypesAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<EquipmentTypeDto?> GetEquipmentTypeByIdAsync(int id, CancellationToken ct = default);
    Task<EquipmentTypeDto> CreateEquipmentTypeAsync(EquipmentTypeCreateDto dto, CancellationToken ct = default);
    Task UpdateEquipmentTypeAsync(EquipmentTypeUpdateDto dto, CancellationToken ct = default);
    Task<DeleteResult> DeleteOrDeactivateEquipmentTypeAsync(int id, CancellationToken ct = default);

    Task<List<EquipmentSubTypeDto>> GetSubTypesByTypeAsync(int equipmentTypeId, bool includeInactive = false, CancellationToken ct = default);
    Task<EquipmentSubTypeDto> CreateSubTypeAsync(EquipmentSubTypeCreateDto dto, CancellationToken ct = default);
    Task UpdateSubTypeAsync(EquipmentSubTypeUpdateDto dto, CancellationToken ct = default);
    Task<DeleteResult> DeleteOrDeactivateSubTypeAsync(int id, CancellationToken ct = default);

    Task<List<ChecklistItemTemplateDto>> GetChecklistTemplatesAsync(int equipmentTypeId, bool includeInactive = false, CancellationToken ct = default);
    Task<ChecklistItemTemplateDto> CreateChecklistTemplateAsync(ChecklistItemTemplateCreateDto dto, CancellationToken ct = default);
    Task UpdateChecklistTemplateAsync(ChecklistItemTemplateUpdateDto dto, CancellationToken ct = default);
    Task<DeleteResult> DeleteOrDeactivateChecklistTemplateAsync(int id, CancellationToken ct = default);

    // ── Inventory ──
    Task<List<InventoryEquipmentDto>> GetInventoryAsync(
        int? equipmentTypeId = null, EquipmentStatus? status = null,
        string? complianceFilter = null, string? searchTerm = null,
        bool? isAssigned = null, CancellationToken ct = default);
    Task<InventoryStatsDto> GetInventoryStatsAsync(CancellationToken ct = default);
    Task<EquipmentDetailDto?> GetEquipmentDetailAsync(int equipmentId, CancellationToken ct = default);
    Task<List<EquipmentDto>> CreateInventoryEquipmentBulkAsync(EquipmentInventoryCreateDto dto, string userId, CancellationToken ct = default);
    Task AssignEquipmentAsync(AssignEquipmentDto dto, string userId, CancellationToken ct = default);
    Task UnassignEquipmentAsync(UnassignEquipmentDto dto, string userId, CancellationToken ct = default);
    Task BulkAssignEquipmentAsync(List<int> equipmentIds, int sectionId, string userId, CancellationToken ct = default);
    Task UpdateEquipmentStatusAsync(EquipmentStatusUpdateDto dto, string userId, CancellationToken ct = default);

    // ── Equipment Check Templates ──
    Task<List<EquipmentCheckDto>> GetEquipmentChecksAsync(int equipmentTypeId, bool includeInactive = false, CancellationToken ct = default);
    Task<EquipmentCheckDto> CreateEquipmentCheckAsync(EquipmentCheckCreateDto dto, CancellationToken ct = default);
    Task UpdateEquipmentCheckAsync(EquipmentCheckUpdateDto dto, CancellationToken ct = default);
    Task<DeleteResult> DeleteOrDeactivateEquipmentCheckAsync(int id, CancellationToken ct = default);
    Task<List<EquipmentCheckDto>> GetApplicableChecksAsync(int equipmentTypeId, int? equipmentSubTypeId = null, CancellationToken ct = default);

    // ── Check Records ──
    Task<List<EquipmentCheckRecordDto>> GetCheckRecordsAsync(int equipmentId, CancellationToken ct = default);
    Task<EquipmentCheckRecordDto> UpsertCheckRecordAsync(EquipmentCheckRecordUpsertDto dto, string userId, CancellationToken ct = default);
    Task BulkUpsertCheckRecordsAsync(int equipmentId, List<EquipmentCheckRecordUpsertDto> records, string userId, CancellationToken ct = default);
}
