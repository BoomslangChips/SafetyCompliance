namespace SafetyCompliance.Application.DTOs;

public record SectionDto(int Id, int PlantId, string PlantName, string Name, string? Description, int SortOrder, bool IsActive, int EquipmentCount);
public record SectionCreateDto(int PlantId, string Name, string? Description, int SortOrder);
public record SectionUpdateDto(int Id, string Name, string? Description, int SortOrder, bool IsActive);
