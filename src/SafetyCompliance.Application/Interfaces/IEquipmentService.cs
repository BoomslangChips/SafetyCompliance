using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IEquipmentService
{
    Task<List<EquipmentDto>> GetEquipmentBySectionAsync(int sectionId, CancellationToken ct = default);
    Task<EquipmentDto?> GetEquipmentByIdAsync(int id, CancellationToken ct = default);
    Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateEquipmentAsync(EquipmentUpdateDto dto, string userId, CancellationToken ct = default);

    Task<List<EquipmentTypeDto>> GetEquipmentTypesAsync(CancellationToken ct = default);
    Task<EquipmentTypeDto?> GetEquipmentTypeByIdAsync(int id, CancellationToken ct = default);
    Task<EquipmentTypeDto> CreateEquipmentTypeAsync(EquipmentTypeCreateDto dto, CancellationToken ct = default);
    Task UpdateEquipmentTypeAsync(EquipmentTypeUpdateDto dto, CancellationToken ct = default);

    Task<List<EquipmentSubTypeDto>> GetSubTypesByTypeAsync(int equipmentTypeId, CancellationToken ct = default);
    Task<EquipmentSubTypeDto> CreateSubTypeAsync(EquipmentSubTypeCreateDto dto, CancellationToken ct = default);

    Task<List<ChecklistItemTemplateDto>> GetChecklistTemplatesAsync(int equipmentTypeId, CancellationToken ct = default);
    Task<ChecklistItemTemplateDto> CreateChecklistTemplateAsync(ChecklistItemTemplateCreateDto dto, CancellationToken ct = default);
    Task UpdateChecklistTemplateAsync(ChecklistItemTemplateUpdateDto dto, CancellationToken ct = default);
}
