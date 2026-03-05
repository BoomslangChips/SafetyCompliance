using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class EquipmentCheckRecord : AuditableEntity
{
    public int EquipmentId { get; set; }
    public int EquipmentCheckId { get; set; }
    public DateOnly DateValue { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Notes { get; set; }

    public Equipment Equipment { get; set; } = null!;
    public EquipmentCheck EquipmentCheck { get; set; } = null!;
}
