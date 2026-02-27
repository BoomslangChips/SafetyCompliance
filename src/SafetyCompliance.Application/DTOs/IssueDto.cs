using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.DTOs;

public record IssueDto(
    int Id, string Title, string? Description, IssuePriority Priority, IssueStatus Status,
    string? AssignedTo, DateOnly? DueDate, DateTime? ResolvedAt, string? ResolvedByName,
    int? InspectionRoundId, int? EquipmentInspectionId, int? EquipmentId,
    string? EquipmentIdentifier, string? EquipmentTypeName,
    string? PhotoBase64, string? PhotoFileName,
    string CreatedByName, DateTime CreatedAt, int CommentCount);

public record IssueCreateDto(
    string Title, string? Description, IssuePriority Priority,
    string? AssignedTo, DateOnly? DueDate,
    int? InspectionRoundId, int? EquipmentInspectionId, int? EquipmentId,
    string? PhotoBase64, string? PhotoFileName);

public record IssueUpdateDto(
    int Id, string Title, string? Description, IssuePriority Priority,
    IssueStatus Status, string? AssignedTo, DateOnly? DueDate);

public record IssueResolveDto(int Id, string? ResolutionNotes);
