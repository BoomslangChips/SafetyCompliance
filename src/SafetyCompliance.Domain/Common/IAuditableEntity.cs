namespace SafetyCompliance.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    string CreatedById { get; set; }
    DateTime? ModifiedAt { get; set; }
    string? ModifiedById { get; set; }
}
