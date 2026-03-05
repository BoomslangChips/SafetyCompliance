namespace SafetyCompliance.Application.DTOs;

public record EquipmentTypeDto(int Id, string Name, string? Description, bool IsActive, int ChecklistItemCount, int SubTypeCount, int EquipmentCheckCount);
public record EquipmentTypeCreateDto(string Name, string? Description);
public record EquipmentTypeUpdateDto(int Id, string Name, string? Description, bool IsActive);

public record EquipmentSubTypeDto(int Id, int EquipmentTypeId, string Name, bool IsActive);
public record EquipmentSubTypeCreateDto(int EquipmentTypeId, string Name);
public record EquipmentSubTypeUpdateDto(int Id, string Name, bool IsActive);

public enum DeleteOutcome { Deleted, Deactivated, Blocked }
public record DeleteResult(DeleteOutcome Outcome, string Message);

public record ChecklistItemTemplateDto(int Id, int EquipmentTypeId, int? EquipmentSubTypeId, string? EquipmentSubTypeName, string ItemName, string? Description, int SortOrder, bool IsRequired, bool IsActive);
public record ChecklistItemTemplateCreateDto(int EquipmentTypeId, string ItemName, string? Description, int SortOrder, bool IsRequired, int? EquipmentSubTypeId = null);
public record ChecklistItemTemplateUpdateDto(int Id, string ItemName, string? Description, int SortOrder, bool IsRequired, bool IsActive);

// ── Equipment Check (compliance date template) DTOs ──

public record EquipmentCheckDto(
    int Id, int EquipmentTypeId, int? EquipmentSubTypeId, string? EquipmentSubTypeName,
    string Name, string? Description, int? IntervalMonths, bool IsRequired,
    int SortOrder, bool IsActive);

public record EquipmentCheckCreateDto(
    int EquipmentTypeId, string Name, string? Description,
    int? IntervalMonths, bool IsRequired, int SortOrder,
    int? EquipmentSubTypeId = null);

public record EquipmentCheckUpdateDto(
    int Id, string Name, string? Description,
    int? IntervalMonths, bool IsRequired, int SortOrder, bool IsActive);
