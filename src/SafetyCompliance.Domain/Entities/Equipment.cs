using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class Equipment : AuditableEntity
{
    public int SectionId { get; set; }
    public int EquipmentTypeId { get; set; }
    public int? EquipmentSubTypeId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Size { get; set; }
    public string? SerialNumber { get; set; }
    public DateOnly? InstallDate { get; set; }
    public DateOnly? LastServiceDate { get; set; }
    public DateOnly? NextServiceDate { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Section Section { get; set; } = null!;
    public EquipmentType EquipmentType { get; set; } = null!;
    public EquipmentSubType? EquipmentSubType { get; set; }
    public ICollection<EquipmentInspection> EquipmentInspections { get; set; } = [];
}
