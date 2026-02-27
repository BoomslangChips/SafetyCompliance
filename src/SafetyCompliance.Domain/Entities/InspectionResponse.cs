using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class InspectionResponse : BaseEntity
{
    public int EquipmentInspectionId { get; set; }
    public int ChecklistItemTemplateId { get; set; }
    public bool? Response { get; set; }
    public string? Comment { get; set; }

    public EquipmentInspection EquipmentInspection { get; set; } = null!;
    public ChecklistItemTemplate ChecklistItemTemplate { get; set; } = null!;
}
