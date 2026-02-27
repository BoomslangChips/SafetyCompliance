namespace SafetyCompliance.Application.DTOs;

public record EquipmentTypeDto(int Id, string Name, string? Description, bool IsActive, int ChecklistItemCount, int SubTypeCount);
public record EquipmentTypeCreateDto(string Name, string? Description);
public record EquipmentTypeUpdateDto(int Id, string Name, string? Description, bool IsActive);

public record EquipmentSubTypeDto(int Id, int EquipmentTypeId, string Name, bool IsActive);
public record EquipmentSubTypeCreateDto(int EquipmentTypeId, string Name);

public record ChecklistItemTemplateDto(int Id, int EquipmentTypeId, string ItemName, string? Description, int SortOrder, bool IsRequired, bool IsActive);
public record ChecklistItemTemplateCreateDto(int EquipmentTypeId, string ItemName, string? Description, int SortOrder, bool IsRequired);
public record ChecklistItemTemplateUpdateDto(int Id, string ItemName, string? Description, int SortOrder, bool IsRequired, bool IsActive);
