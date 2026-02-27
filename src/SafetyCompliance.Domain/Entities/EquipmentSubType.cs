using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class EquipmentSubType : BaseEntity
{
    public int EquipmentTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public EquipmentType EquipmentType { get; set; } = null!;
}
