namespace SafetyCompliance.Application.Interfaces;

public interface ISyncService
{
    bool IsOnline { get; }
    bool IsSyncing { get; }
    int PendingChangeCount { get; }
    DateTime? LastSyncTime { get; }
    string? LastSyncError { get; }

    event Action? OnSyncStateChanged;

    Task<SyncResult> SyncAsync(CancellationToken ct = default);
    Task<bool> TestConnectionAsync();
    void StartAutoSync();
    void StopAutoSync();
}

public record SyncResult(bool Success, int PulledCount, int PushedCount, string? Error);
