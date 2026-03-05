using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class EquipmentCheck : BaseEntity
{
    public int EquipmentTypeId { get; set; }
    public int? EquipmentSubTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? IntervalMonths { get; set; }
    public bool IsRequired { get; set; } = true;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public EquipmentType EquipmentType { get; set; } = null!;
    public EquipmentSubType? EquipmentSubType { get; set; }
    public ICollection<EquipmentCheckRecord> CheckRecords { get; set; } = [];
}
