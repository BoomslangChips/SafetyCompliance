using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class EquipmentType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChecklistItemTemplate> ChecklistItemTemplates { get; set; } = [];
    public ICollection<EquipmentSubType> SubTypes { get; set; } = [];
    public ICollection<Equipment> Equipment { get; set; } = [];
}
