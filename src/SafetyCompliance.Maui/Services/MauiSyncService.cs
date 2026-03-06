using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Common;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SafetyCompliance.Maui.Services;

public class MauiSyncService : ISyncService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _syncConnectionString;
    private readonly ILogger<MauiSyncService> _logger;
    private Timer? _autoSyncTimer;
    private bool _isSyncing;
    private int _pendingChangeCount;
    private DateTime? _lastSyncTime;
    private string? _lastSyncError;
    private bool _isOnline;

    public bool IsOnline => _isOnline;
    public bool IsSyncing => _isSyncing;
    public int PendingChangeCount => _pendingChangeCount;
    public DateTime? LastSyncTime => _lastSyncTime;
    public string? LastSyncError => _lastSyncError;

    public event Action? OnSyncStateChanged;

    public MauiSyncService(IServiceScopeFactory scopeFactory, string syncConnectionString)
    {
        _scopeFactory = scopeFactory;
        _syncConnectionString = syncConnectionString;
        _logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<MauiSyncService>>();

        // Check initial pending count
        _ = RefreshPendingCountAsync();
    }

    public void StartAutoSync()
    {
        _autoSyncTimer?.Dispose();
        _autoSyncTimer = new Timer(async _ =>
        {
            if (!_isSyncing)
                await SyncAsync();
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(5));
    }

    public void StopAutoSync()
    {
        _autoSyncTimer?.Dispose();
        _autoSyncTimer = null;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var serverCtx = CreateServerContext();
            var canConnect = await serverCtx.Database.CanConnectAsync();
            _isOnline = canConnect;
            OnSyncStateChanged?.Invoke();
            return canConnect;
        }
        catch
        {
            _isOnline = false;
            OnSyncStateChanged?.Invoke();
            return false;
        }
    }

    public async Task<SyncResult> SyncAsync(CancellationToken ct = default)
    {
        if (_isSyncing)
            return new SyncResult(false, 0, 0, "Sync already in progress");

        _isSyncing = true;
        _lastSyncError = null;
        OnSyncStateChanged?.Invoke();

        try
        {
            // Test connection first
            if (!await TestConnectionAsync())
                return new SyncResult(false, 0, 0, "Cannot reach server");

            // Push local changes first, then pull
            var pushed = await PushAsync(ct);
            var pulled = await PullAsync(ct);

            _lastSyncTime = DateTime.Now;
            _lastSyncError = null;
            await RefreshPendingCountAsync();

            return new SyncResult(true, pulled, pushed, null);
        }
        catch (Exception ex)
        {
            _lastSyncError = ex.Message;
            _logger.LogError(ex, "Sync failed");
            return new SyncResult(false, 0, 0, ex.Message);
        }
        finally
        {
            _isSyncing = false;
            OnSyncStateChanged?.Invoke();
        }
    }

    private async Task<int> PullAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var localCtx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        localCtx.SuppressChangeTracking = true; // Don't log pulled data as pending changes

        using var serverCtx = CreateServerContext();
        int total = 0;

        // Pull in FK dependency order (reference data first, then transactional)
        total += await PullTable<Company>(serverCtx, localCtx, ct);
        total += await PullTable<Plant>(serverCtx, localCtx, ct);
        total += await PullTable<Section>(serverCtx, localCtx, ct);
        total += await PullTable<EquipmentType>(serverCtx, localCtx, ct);
        total += await PullTable<EquipmentSubType>(serverCtx, localCtx, ct);
        total += await PullTable<ChecklistItemTemplate>(serverCtx, localCtx, ct);
        total += await PullTable<EquipmentCheck>(serverCtx, localCtx, ct);
        total += await PullTable<InspectionSchedule>(serverCtx, localCtx, ct);
        total += await PullTable<Equipment>(serverCtx, localCtx, ct);
        total += await PullTable<InspectionRound>(serverCtx, localCtx, ct);
        total += await PullTable<EquipmentInspection>(serverCtx, localCtx, ct);
        total += await PullTable<InspectionResponse>(serverCtx, localCtx, ct);
        total += await PullTable<InspectionPhoto>(serverCtx, localCtx, ct);
        total += await PullTable<Issue>(serverCtx, localCtx, ct);
        total += await PullTable<Comment>(serverCtx, localCtx, ct);
        total += await PullTable<ServiceBooking>(serverCtx, localCtx, ct);
        total += await PullTable<Note>(serverCtx, localCtx, ct);
        total += await PullTable<PlantContact>(serverCtx, localCtx, ct);
        total += await PullTable<ContactDocument>(serverCtx, localCtx, ct);
        total += await PullTable<EquipmentCheckRecord>(serverCtx, localCtx, ct);

        // Update sync meta
        var meta = await localCtx.SyncMetas.FirstOrDefaultAsync(m => m.TableName == "_global", ct);
        if (meta is null)
        {
            localCtx.SyncMetas.Add(new SyncMeta
            {
                TableName = "_global",
                LastPulledAt = DateTime.UtcNow,
                LastPushedAt = DateTime.MinValue
            });
        }
        else
        {
            meta.LastPulledAt = DateTime.UtcNow;
        }
        await localCtx.SaveChangesAsync(ct);

        _logger.LogInformation("Pull complete: {Total} records synced", total);
        return total;
    }

    private async Task<int> PullTable<T>(
        ApplicationDbContext serverCtx,
        ApplicationDbContext localCtx,
        CancellationToken ct) where T : BaseEntity
    {
        try
        {
            var serverItems = await serverCtx.Set<T>().AsNoTracking().ToListAsync(ct);
            if (serverItems.Count == 0) return 0;

            var localIds = await localCtx.Set<T>()
                .Select(e => e.Id)
                .ToHashSetAsync(ct);

            int count = 0;
            foreach (var item in serverItems)
            {
                if (localIds.Contains(item.Id))
                {
                    // Update existing
                    var existing = await localCtx.Set<T>().FindAsync([item.Id], ct);
                    if (existing is not null)
                        localCtx.Entry(existing).CurrentValues.SetValues(item);
                }
                else
                {
                    // Insert new
                    localCtx.Set<T>().Add(item);
                }
                count++;
            }

            // Delete local records that no longer exist on server
            var serverIds = serverItems.Select(i => i.Id).ToHashSet();
            var toDelete = await localCtx.Set<T>()
                .Where(e => !serverIds.Contains(e.Id))
                .ToListAsync(ct);
            if (toDelete.Count > 0)
                localCtx.Set<T>().RemoveRange(toDelete);

            await localCtx.SaveChangesAsync(ct);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to pull table {Table}", typeof(T).Name);
            // Clear change tracker to prevent cascading failures in subsequent tables
            localCtx.ChangeTracker.Clear();
            return 0;
        }
    }

    private async Task<int> PushAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var localCtx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var pendingChanges = await localCtx.PendingChanges
            .OrderBy(p => p.ChangedAt)
            .ToListAsync(ct);

        if (pendingChanges.Count == 0) return 0;

        using var serverCtx = CreateServerContext();
        int pushed = 0;

        foreach (var change in pendingChanges)
        {
            try
            {
                var success = await PushSingleChange(localCtx, serverCtx, change, ct);
                if (success)
                {
                    localCtx.PendingChanges.Remove(change);
                    pushed++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to push change {Table} #{Id} ({Type})",
                    change.TableName, change.RecordId, change.ChangeType);
                // Clear server context to prevent cascading failures
                serverCtx.ChangeTracker.Clear();
            }
        }

        localCtx.SuppressChangeTracking = true;
        await localCtx.SaveChangesAsync(ct);

        _logger.LogInformation("Push complete: {Pushed}/{Total} changes", pushed, pendingChanges.Count);
        return pushed;
    }

    private async Task<bool> PushSingleChange(
        ApplicationDbContext localCtx,
        ApplicationDbContext serverCtx,
        PendingChange change,
        CancellationToken ct)
    {
        // Map table name to entity type and push
        return change.TableName switch
        {
            "Companies" => await PushEntity<Company>(localCtx, serverCtx, change, ct),
            "Plants" => await PushEntity<Plant>(localCtx, serverCtx, change, ct),
            "Sections" => await PushEntity<Section>(localCtx, serverCtx, change, ct),
            "EquipmentTypes" => await PushEntity<EquipmentType>(localCtx, serverCtx, change, ct),
            "EquipmentSubTypes" => await PushEntity<EquipmentSubType>(localCtx, serverCtx, change, ct),
            "ChecklistItemTemplates" => await PushEntity<ChecklistItemTemplate>(localCtx, serverCtx, change, ct),
            "EquipmentChecks" => await PushEntity<EquipmentCheck>(localCtx, serverCtx, change, ct),
            "InspectionSchedules" => await PushEntity<InspectionSchedule>(localCtx, serverCtx, change, ct),
            "Equipment" => await PushEntity<Equipment>(localCtx, serverCtx, change, ct),
            "InspectionRounds" => await PushEntity<InspectionRound>(localCtx, serverCtx, change, ct),
            "EquipmentInspections" => await PushEntity<EquipmentInspection>(localCtx, serverCtx, change, ct),
            "InspectionResponses" => await PushEntity<InspectionResponse>(localCtx, serverCtx, change, ct),
            "InspectionPhotos" => await PushEntity<InspectionPhoto>(localCtx, serverCtx, change, ct),
            "Issues" => await PushEntity<Issue>(localCtx, serverCtx, change, ct),
            "Comments" => await PushEntity<Comment>(localCtx, serverCtx, change, ct),
            "ServiceBookings" => await PushEntity<ServiceBooking>(localCtx, serverCtx, change, ct),
            "Notes" => await PushEntity<Note>(localCtx, serverCtx, change, ct),
            "PlantContacts" => await PushEntity<PlantContact>(localCtx, serverCtx, change, ct),
            "ContactDocuments" => await PushEntity<ContactDocument>(localCtx, serverCtx, change, ct),
            "EquipmentCheckRecords" => await PushEntity<EquipmentCheckRecord>(localCtx, serverCtx, change, ct),
            _ => true // Unknown table, skip and remove from queue
        };
    }

    private async Task<bool> PushEntity<T>(
        ApplicationDbContext localCtx,
        ApplicationDbContext serverCtx,
        PendingChange change,
        CancellationToken ct) where T : BaseEntity
    {
        if (change.ChangeType == "Deleted")
        {
            var serverEntity = await serverCtx.Set<T>().FindAsync([change.RecordId], ct);
            if (serverEntity is not null)
            {
                serverCtx.Set<T>().Remove(serverEntity);
                await serverCtx.SaveChangesAsync(ct);
            }
            return true;
        }

        // Insert or Update
        var localEntity = await localCtx.Set<T>().AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == change.RecordId, ct);

        if (localEntity is null)
            return true; // Entity was deleted locally after change was queued

        var existing = await serverCtx.Set<T>().FindAsync([localEntity.Id], ct);
        if (existing is not null)
        {
            // Update
            serverCtx.Entry(existing).CurrentValues.SetValues(localEntity);
        }
        else
        {
            // Insert — enable IDENTITY_INSERT for this table
            await serverCtx.Database.ExecuteSqlRawAsync(
                $"SET IDENTITY_INSERT [{change.TableName}] ON", ct);
            try
            {
                serverCtx.Set<T>().Add(localEntity);
                await serverCtx.SaveChangesAsync(ct);
            }
            finally
            {
                await serverCtx.Database.ExecuteSqlRawAsync(
                    $"SET IDENTITY_INSERT [{change.TableName}] OFF", ct);
            }
            return true;
        }

        await serverCtx.SaveChangesAsync(ct);
        return true;
    }

    private ApplicationDbContext CreateServerContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_syncConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    private async Task RefreshPendingCountAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _pendingChangeCount = await ctx.PendingChanges.CountAsync();
        }
        catch
        {
            _pendingChangeCount = 0;
        }
    }

    public void Dispose()
    {
        _autoSyncTimer?.Dispose();
    }
}

internal static class AsyncEnumerableExtensions
{
    public static async Task<HashSet<T>> ToHashSetAsync<T>(
        this IQueryable<T> query, CancellationToken ct = default)
    {
        var set = new HashSet<T>();
        await foreach (var item in query.AsAsyncEnumerable().WithCancellation(ct))
            set.Add(item);
        return set;
    }
}
