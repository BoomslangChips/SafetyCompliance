using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class InspectionSchedule : AuditableEntity
{
    public int PlantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FrequencyType Frequency { get; set; }
    public int FrequencyInterval { get; set; } = 1;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly NextDueDate { get; set; }
    public DateOnly? LastCompletedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AutoGenerate { get; set; } = true;

    public Plant Plant { get; set; } = null!;
    public ICollection<InspectionRound> InspectionRounds { get; set; } = [];
}

public enum FrequencyType : byte
{
    Daily = 0,
    Weekly = 1,
    BiWeekly = 2,
    Monthly = 3,
    Quarterly = 4,
    SemiAnnually = 5,
    Annually = 6
}
