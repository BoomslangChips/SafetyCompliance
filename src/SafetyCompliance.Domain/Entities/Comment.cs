using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class Comment : BaseEntity
{
    public int? InspectionRoundId { get; set; }
    public int? IssueId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? PhotoBase64 { get; set; }
    public string? PhotoFileName { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public InspectionRound? InspectionRound { get; set; }
    public Issue? Issue { get; set; }
    public ApplicationUser CreatedBy { get; set; } = null!;
}
