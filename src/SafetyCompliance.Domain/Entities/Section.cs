using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class Section : AuditableEntity
{
    public int PlantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? PhotoBase64 { get; set; }
    public string? PhotoFileName { get; set; }

    public Plant Plant { get; set; } = null!;
    public ICollection<Equipment> Equipment { get; set; } = [];
}
