using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class ChecklistItemTemplate : BaseEntity
{
    public int EquipmentTypeId { get; set; }

    /// <summary>
    /// When null: this item applies to ALL sub-types of the equipment type.
    /// When set: this item applies ONLY to this specific sub-type.
    /// During inspection the sub-type-specific set takes precedence over the
    /// type-level (null) set — so DCP extinguishers use the null items and
    /// CO2 extinguishers use the CO2-specific items.
    /// </summary>
    public int? EquipmentSubTypeId { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public EquipmentType EquipmentType { get; set; } = null!;
    public EquipmentSubType? EquipmentSubType { get; set; }
}
