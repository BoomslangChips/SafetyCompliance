namespace SafetyCompliance.Application.DTOs;

public record PlantDto(int Id, int CompanyId, string CompanyName, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail, bool IsActive, int SectionCount, int EquipmentCount);
public record PlantCreateDto(int CompanyId, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail);
public record PlantUpdateDto(int Id, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail, bool IsActive);
