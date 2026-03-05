namespace SafetyCompliance.Application.DTOs;

public record EquipmentDto(
    int Id, int? SectionId, string? SectionName, int EquipmentTypeId, string EquipmentTypeName,
    int? EquipmentSubTypeId, string? SubTypeName, string Identifier, string? Description,
    string? Size, string? SerialNumber, DateOnly? NextServiceDate, int SortOrder, bool IsActive);

public record EquipmentCreateDto(
    int? SectionId, int EquipmentTypeId, int? EquipmentSubTypeId, string Identifier,
    string? Description, string? Size, string? SerialNumber, DateOnly? InstallDate,
    DateOnly? NextServiceDate, int SortOrder);

public record EquipmentUpdateDto(
    int Id, int EquipmentTypeId, int? EquipmentSubTypeId, string Identifier,
    string? Description, string? Size, string? SerialNumber, DateOnly? NextServiceDate,
    int SortOrder, bool IsActive);

// ── Inventory DTOs ──

public record InventoryGroupDto(
    int EquipmentTypeId, string EquipmentTypeName,
    int? EquipmentSubTypeId, string? SubTypeName,
    int TotalCount, int AssignedCount, int AvailableCount,
    List<InventoryEquipmentDto> Equipment);

public record InventoryEquipmentDto(
    int Id, string Identifier, string? Description, string? Size,
    string? SerialNumber, bool IsActive,
    int? SectionId, string? SectionName,
    int? PlantId, string? PlantName,
    int? CompanyId, string? CompanyName,
    int EquipmentTypeId, string EquipmentTypeName,
    int? EquipmentSubTypeId, string? SubTypeName);

public record AssignEquipmentDto(int EquipmentId, int SectionId);
public record UnassignEquipmentDto(int EquipmentId);

public record EquipmentInventoryCreateDto(
    int EquipmentTypeId, int? EquipmentSubTypeId, string Identifier,
    string? Description, string? Size, string? SerialNumber);

// ── Check Record DTOs ──

public record EquipmentCheckRecordDto(
    int Id, int EquipmentId, int EquipmentCheckId, string CheckName,
    int? IntervalMonths, DateOnly DateValue, DateOnly? ExpiryDate,
    string? Notes, string ComplianceStatus);

public record EquipmentCheckRecordUpsertDto(
    int EquipmentId, int EquipmentCheckId, DateOnly DateValue, string? Notes);
