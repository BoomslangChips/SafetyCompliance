using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class InspectionRound : BaseEntity
{
    public int PlantId { get; set; }
    public int? InspectionScheduleId { get; set; }
    public DateOnly InspectionDate { get; set; }
    public string InspectionMonth { get; set; } = string.Empty;
    public InspectionStatus Status { get; set; } = InspectionStatus.Draft;
    public string? Notes { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InspectedById { get; set; } = string.Empty;
    public string? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Plant Plant { get; set; } = null!;
    public InspectionSchedule? InspectionSchedule { get; set; }
    public ApplicationUser InspectedBy { get; set; } = null!;
    public ApplicationUser? ReviewedBy { get; set; }
    public ICollection<EquipmentInspection> EquipmentInspections { get; set; } = [];
    public ICollection<Issue> Issues { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}

public enum InspectionStatus : byte
{
    Draft = 0,
    InProgress = 1,
    Completed = 2,
    Reviewed = 3
}
