namespace SafetyCompliance.Application.DTOs;

public record PlantDto(int Id, int CompanyId, string CompanyName, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail, bool IsActive, int SectionCount, int EquipmentCount, string? PhotoBase64 = null, string? PhotoFileName = null);
public record PlantCreateDto(int CompanyId, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail, string? PhotoBase64 = null, string? PhotoFileName = null);
public record PlantUpdateDto(int Id, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail, bool IsActive, string? PhotoBase64 = null, string? PhotoFileName = null);
