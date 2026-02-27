using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class InspectionPhoto : BaseEntity
{
    public int EquipmentInspectionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Caption { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedById { get; set; } = string.Empty;

    public EquipmentInspection EquipmentInspection { get; set; } = null!;
    public ApplicationUser UploadedBy { get; set; } = null!;
}
