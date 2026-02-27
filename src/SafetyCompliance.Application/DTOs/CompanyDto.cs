namespace SafetyCompliance.Application.DTOs;

public record CompanyDto(int Id, string Name, string? Code, string? Address, string? ContactName, string? ContactEmail, string? ContactPhone, bool IsActive, int PlantCount, int TotalEquipment, string? PhotoBase64 = null, string? PhotoFileName = null);
public record CompanyCreateDto(string Name, string? Code, string? Address, string? ContactName, string? ContactEmail, string? ContactPhone, string? PhotoBase64 = null, string? PhotoFileName = null);
public record CompanyUpdateDto(int Id, string Name, string? Code, string? Address, string? ContactName, string? ContactEmail, string? ContactPhone, bool IsActive, string? PhotoBase64 = null, string? PhotoFileName = null);
