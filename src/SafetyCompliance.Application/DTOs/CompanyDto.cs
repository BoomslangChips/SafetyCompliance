namespace SafetyCompliance.Application.DTOs;

public record CompanyDto(int Id, string Name, string? Code, string? Address, string? ContactName, string? ContactEmail, string? ContactPhone, bool IsActive, int PlantCount, int TotalEquipment);
public record CompanyCreateDto(string Name, string? Code, string? Address, string? ContactName, string? ContactEmail, string? ContactPhone);
public record CompanyUpdateDto(int Id, string Name, string? Code, string? Address, string? ContactName, string? ContactEmail, string? ContactPhone, bool IsActive);
