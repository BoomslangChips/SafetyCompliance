using SafetyCompliance.Domain.Common;

namespace SafetyCompliance.Domain.Entities;

/// <summary>
/// Tracks local changes made on the mobile device that need to be pushed to the server.
/// </summary>
public class PendingChange : BaseEntity
{
    public string TableName { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public string ChangeType { get; set; } = string.Empty; // "Insert", "Update", "Delete"
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Payload { get; set; } // JSON snapshot for deletes
}

/// <summary>
/// Tracks per-table sync timestamps for incremental pull/push.
/// </summary>
public class SyncMeta : BaseEntity
{
    public string TableName { get; set; } = string.Empty;
    public DateTime LastPulledAt { get; set; } = DateTime.MinValue;
    public DateTime LastPushedAt { get; set; } = DateTime.MinValue;
}
