using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class ChecklistItemTemplate : BaseEntity
{
    public int EquipmentTypeId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public EquipmentType EquipmentType { get; set; } = null!;
}
