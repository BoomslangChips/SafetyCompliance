using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class Issue : AuditableEntity
{
    public int? InspectionRoundId { get; set; }
    public int? EquipmentInspectionId { get; set; }
    public int? EquipmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    public IssueStatus Status { get; set; } = IssueStatus.Open;
    public string? AssignedTo { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedById { get; set; }
    public string? PhotoBase64 { get; set; }
    public string? PhotoFileName { get; set; }

    public InspectionRound? InspectionRound { get; set; }
    public EquipmentInspection? EquipmentInspection { get; set; }
    public Equipment? Equipment { get; set; }
    public ICollection<Comment> Comments { get; set; } = [];
}

public enum IssuePriority : byte
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum IssueStatus : byte
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}
