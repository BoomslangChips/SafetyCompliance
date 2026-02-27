using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class ServiceBooking : AuditableEntity
{
    public int EquipmentId { get; set; }
    public int? EquipmentInspectionId { get; set; }
    public string ServiceProvider { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public ServiceBookingStatus Status { get; set; } = ServiceBookingStatus.Sent;
    public DateOnly SentDate { get; set; }
    public DateOnly? ExpectedReturnDate { get; set; }
    public DateOnly? ActualReturnDate { get; set; }
    public string? Notes { get; set; }

    public Equipment Equipment { get; set; } = null!;
    public EquipmentInspection? EquipmentInspection { get; set; }
}

public enum ServiceBookingStatus : byte
{
    Sent = 0,
    InService = 1,
    Returned = 2,
    Cancelled = 3
}
