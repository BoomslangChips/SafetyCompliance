namespace SafetyCompliance.Application.DTOs;

public record SetupEquipmentItem(
    string Identifier,
    int EquipmentTypeId,
    int? EquipmentSubTypeId,
    string? Description,
    string? Size,
    string? SerialNumber);

public record SetupSectionItem(
    string Name,
    string? Description,
    string? PhotoBase64,
    string? PhotoFileName,
    List<SetupEquipmentItem> Equipment);

public record SetupPlantItem(
    string Name,
    string? Description,
    string? ContactName,
    string? ContactPhone,
    string? ContactEmail,
    string? PhotoBase64,
    string? PhotoFileName,
    List<SetupSectionItem> Sections);

public record SetupCreateDto(
    int? CompanyId,
    string? CompanyName,
    string? CompanyCode,
    string? CompanyAddress,
    string? CompanyContactName,
    string? CompanyContactEmail,
    string? CompanyContactPhone,
    string? CompanyPhotoBase64,
    string? CompanyPhotoFileName,
    List<SetupPlantItem> Plants);
