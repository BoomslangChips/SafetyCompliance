using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

public class ContactDocument : AuditableEntity
{
    public int    PlantContactId { get; set; }
    public PlantContact PlantContact { get; set; } = null!;

    /// <summary>Original file name as uploaded.</summary>
    public string FileName    { get; set; } = string.Empty;

    /// <summary>User-renameable display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>MIME type, e.g. application/pdf.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Size in bytes.</summary>
    public long   FileSizeBytes { get; set; }

    /// <summary>Base64 encoded file content stored in DB.</summary>
    public string FileBase64  { get; set; } = string.Empty;
}
