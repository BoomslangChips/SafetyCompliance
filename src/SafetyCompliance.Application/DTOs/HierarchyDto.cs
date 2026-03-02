namespace SafetyCompliance.Application.DTOs;

public record HierarchyEquipmentDto(int Id, string Identifier, string EquipmentTypeName, string? SubTypeName);

public record HierarchySectionDto(int Id, string Name, string? Description, string? PhotoBase64, int EquipmentCount, List<HierarchyEquipmentDto> Equipment);

public record HierarchyPlantDto(int Id, string Name, string? Description, string? PhotoBase64, int SectionCount, int EquipmentCount, List<HierarchySectionDto> Sections);

public record HierarchyCompanyDto(int Id, string Name, string? Code, string? PhotoBase64, string? Address, string? ContactName, string? ContactEmail, int PlantCount, int TotalEquipment, int TotalSections, List<HierarchyPlantDto> Plants);
