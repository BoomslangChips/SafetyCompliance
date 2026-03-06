using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class Equipment : AuditableEntity
{
    public int? SectionId { get; set; }
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
    public EquipmentStatus Status { get; set; } = EquipmentStatus.InOrder;
    public bool IsActive { get; set; } = true;

    public Section? Section { get; set; }
    public EquipmentType EquipmentType { get; set; } = null!;
    public EquipmentSubType? EquipmentSubType { get; set; }
    public ICollection<EquipmentInspection> EquipmentInspections { get; set; } = [];
    public ICollection<ServiceBooking> ServiceBookings { get; set; } = [];
    public ICollection<EquipmentCheckRecord> CheckRecords { get; set; } = [];
}

public enum EquipmentStatus : byte
{
    InOrder = 0,
    InForService = 1,
    Damaged = 2,
    OutOfPlace = 3,
    NeedsReplacement = 4,
    Retired = 5
}
