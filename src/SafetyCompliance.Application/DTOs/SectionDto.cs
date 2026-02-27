namespace SafetyCompliance.Application.DTOs;

public record SectionDto(int Id, int PlantId, string PlantName, string Name, string? Description, int SortOrder, bool IsActive, int EquipmentCount, string? PhotoBase64 = null, string? PhotoFileName = null);
public record SectionCreateDto(int PlantId, string Name, string? Description, int SortOrder, string? PhotoBase64 = null, string? PhotoFileName = null);
public record SectionUpdateDto(int Id, string Name, string? Description, int SortOrder, bool IsActive, string? PhotoBase64 = null, string? PhotoFileName = null);
