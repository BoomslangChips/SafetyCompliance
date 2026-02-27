namespace SafetyCompliance.Application.DTOs;

public record EquipmentDto(
    int Id, int SectionId, string SectionName, int EquipmentTypeId, string EquipmentTypeName,
    int? EquipmentSubTypeId, string? SubTypeName, string Identifier, string? Description,
    string? Size, string? SerialNumber, DateOnly? NextServiceDate, int SortOrder, bool IsActive);

public record EquipmentCreateDto(
    int SectionId, int EquipmentTypeId, int? EquipmentSubTypeId, string Identifier,
    string? Description, string? Size, string? SerialNumber, DateOnly? InstallDate,
    DateOnly? NextServiceDate, int SortOrder);

public record EquipmentUpdateDto(
    int Id, int EquipmentTypeId, int? EquipmentSubTypeId, string Identifier,
    string? Description, string? Size, string? SerialNumber, DateOnly? NextServiceDate,
    int SortOrder, bool IsActive);
