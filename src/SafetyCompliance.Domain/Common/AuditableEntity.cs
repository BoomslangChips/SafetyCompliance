namespace SafetyCompliance.Domain.Common;

public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedById { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedById { get; set; }
}
