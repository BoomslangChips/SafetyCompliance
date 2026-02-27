namespace SafetyCompliance.Application.DTOs;

public record CommentDto(
    int Id, string Text, string? PhotoBase64, string? PhotoFileName,
    string CreatedByName, DateTime CreatedAt);

public record CommentCreateDto(
    int? InspectionRoundId, int? IssueId, string Text,
    string? PhotoBase64, string? PhotoFileName);
