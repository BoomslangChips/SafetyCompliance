using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.DTOs;

public record EquipmentDto(
    int Id, int? SectionId, string? SectionName, int EquipmentTypeId, string EquipmentTypeName,
    int? EquipmentSubTypeId, string? SubTypeName, string Identifier, string? Description,
    string? Size, string? SerialNumber, DateOnly? NextServiceDate, int SortOrder, bool IsActive,
    EquipmentStatus Status);

public record EquipmentCreateDto(
    int? SectionId, int EquipmentTypeId, int? EquipmentSubTypeId, string Identifier,
    string? Description, string? Size, string? SerialNumber, DateOnly? InstallDate,
    DateOnly? NextServiceDate, int SortOrder);

public record EquipmentUpdateDto(
    int Id, int EquipmentTypeId, int? EquipmentSubTypeId, string Identifier,
    string? Description, string? Size, string? SerialNumber, DateOnly? NextServiceDate,
    int SortOrder, bool IsActive);

// ── Inventory DTOs ──

public record InventoryEquipmentDto(
    int Id, string Identifier, string? Description, string? Size,
    string? SerialNumber, bool IsActive,
    int? SectionId, string? SectionName,
    int? PlantId, string? PlantName,
    int? CompanyId, string? CompanyName,
    int EquipmentTypeId, string EquipmentTypeName,
    int? EquipmentSubTypeId, string? SubTypeName,
    EquipmentStatus Status,
    int TotalChecks, int OverdueChecks, int DueSoonChecks);

public record InventoryStatsDto(
    int TotalEquipment, int AssignedCount, int AvailableCount,
    int InServiceCount, int DamagedCount, int MissingCount,
    int OutOfServiceCount, int NeedsReplacementCount, int RetiredCount,
    int OverdueComplianceCount, int DueSoonComplianceCount);

public record AssignEquipmentDto(int EquipmentId, int SectionId);
public record UnassignEquipmentDto(int EquipmentId);

public record EquipmentInventoryCreateDto(
    int EquipmentTypeId, int? EquipmentSubTypeId, string IdentifierPrefix,
    int Quantity, string? Description, string? Size, string? SerialNumber,
    EquipmentStatus Status = EquipmentStatus.InService);

public record EquipmentStatusUpdateDto(int EquipmentId, EquipmentStatus Status, string? Notes);

// ── Equipment Detail ──

public record EquipmentDetailDto(
    int Id, string Identifier, string? Description, string? Size,
    string? SerialNumber, EquipmentStatus Status, bool IsActive,
    int EquipmentTypeId, string EquipmentTypeName,
    int? EquipmentSubTypeId, string? SubTypeName,
    int? SectionId, string? SectionName,
    int? PlantId, string? PlantName,
    int? CompanyId, string? CompanyName,
    DateOnly? InstallDate, DateOnly? LastServiceDate, DateOnly? NextServiceDate,
    DateTime CreatedAt, DateTime? ModifiedAt,
    List<EquipmentCheckWithRecordDto> ComplianceChecks);

public record EquipmentCheckWithRecordDto(
    int CheckId, string CheckName, string? CheckDescription,
    int? IntervalMonths, bool IsRequired,
    int? RecordId, DateOnly? DateValue, DateOnly? ExpiryDate,
    string? Notes, string ComplianceStatus);

// ── Check Record DTOs ──

public record EquipmentCheckRecordDto(
    int Id, int EquipmentId, int EquipmentCheckId, string CheckName,
    int? IntervalMonths, DateOnly DateValue, DateOnly? ExpiryDate,
    string? Notes, string ComplianceStatus);

public record EquipmentCheckRecordUpsertDto(
    int EquipmentId, int EquipmentCheckId, DateOnly DateValue, string? Notes);
