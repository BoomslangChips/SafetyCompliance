using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class EquipmentInspection : BaseEntity
{
    public int InspectionRoundId { get; set; }
    public int EquipmentId { get; set; }
    public bool IsComplete { get; set; }
    public string? Comments { get; set; }
    public DateTime? InspectedAt { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public InspectionRound InspectionRound { get; set; } = null!;
    public Equipment Equipment { get; set; } = null!;
    public ICollection<InspectionResponse> Responses { get; set; } = [];
    public ICollection<InspectionPhoto> Photos { get; set; } = [];
}
