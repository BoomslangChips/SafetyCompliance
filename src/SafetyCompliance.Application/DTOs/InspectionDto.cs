using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.DTOs;

public record InspectionRoundDto(
    int Id, int PlantId, string PlantName, DateOnly InspectionDate, string InspectionMonth,
    InspectionStatus Status, string InspectorName, int TotalEquipment, int CompletedEquipment,
    DateTime? CompletedAt);

public record EquipmentInspectionDto(
    int Id, int InspectionRoundId, int EquipmentId, string EquipmentIdentifier,
    string EquipmentDescription, string EquipmentTypeName, string? SubTypeName, string? Size,
    bool IsComplete, string? Comments, List<InspectionResponseDto> Responses, int PhotoCount,
    int SectionId, string SectionName);

public record InspectionResponseDto(
    int Id, int ChecklistItemTemplateId, string ItemName, int SortOrder,
    bool? Response, string? Comment);

public record SubmitResponseDto(int EquipmentInspectionId, int ChecklistItemTemplateId, bool Response, string? Comment);
