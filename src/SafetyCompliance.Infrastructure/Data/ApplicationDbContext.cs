using SafetyCompliance.Domain.Common;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Domain.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SafetyCompliance.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IUnitOfWork
{
    private bool _isSqlite;

    /// <summary>
    /// When true, SaveChangesAsync won't log changes to PendingChanges.
    /// Set this during sync pull operations to avoid re-queuing server data.
    /// </summary>
    public bool SuppressChangeTracking { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<EquipmentType> EquipmentTypes => Set<EquipmentType>();
    public DbSet<EquipmentSubType> EquipmentSubTypes => Set<EquipmentSubType>();
    public DbSet<ChecklistItemTemplate> ChecklistItemTemplates => Set<ChecklistItemTemplate>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<InspectionSchedule> InspectionSchedules => Set<InspectionSchedule>();
    public DbSet<InspectionRound> InspectionRounds => Set<InspectionRound>();
    public DbSet<EquipmentInspection> EquipmentInspections => Set<EquipmentInspection>();
    public DbSet<InspectionResponse> InspectionResponses => Set<InspectionResponse>();
    public DbSet<InspectionPhoto> InspectionPhotos => Set<InspectionPhoto>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ServiceBooking> ServiceBookings => Set<ServiceBooking>();
    public DbSet<Note>          Notes            => Set<Note>();
    public DbSet<PlantContact>  PlantContacts    => Set<PlantContact>();
    public DbSet<EquipmentCheck>       EquipmentChecks       => Set<EquipmentCheck>();
    public DbSet<EquipmentCheckRecord> EquipmentCheckRecords => Set<EquipmentCheckRecord>();
    public DbSet<ContactDocument>      ContactDocuments      => Set<ContactDocument>();

    // Sync tracking (used on mobile/SQLite only, but defined here so EF knows about them)
    public DbSet<PendingChange> PendingChanges => Set<PendingChange>();
    public DbSet<SyncMeta>      SyncMetas      => Set<SyncMeta>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // SQLite doesn't support DateOnly natively — convert to/from string
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            _isSqlite = true;

            var dateOnlyConverter = new ValueConverter<DateOnly, string>(
                v => v.ToString("yyyy-MM-dd"),
                v => DateOnly.ParseExact(v, "yyyy-MM-dd"));

            var nullableDateOnlyConverter = new ValueConverter<DateOnly?, string?>(
                v => v.HasValue ? v.Value.ToString("yyyy-MM-dd") : null,
                v => v != null ? DateOnly.ParseExact(v, "yyyy-MM-dd") : null);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateOnly))
                        property.SetValueConverter(dateOnlyConverter);
                    else if (property.ClrType == typeof(DateOnly?))
                        property.SetValueConverter(nullableDateOnlyConverter);
                }
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!_isSqlite || SuppressChangeTracking)
            return await base.SaveChangesAsync(cancellationToken);

        // Capture changes before save (for inserts, Id is 0 until after save)
        var trackedChanges = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is BaseEntity and not PendingChange and not SyncMeta)
            .Select(e => new
            {
                Entity = (BaseEntity)e.Entity,
                Table = e.Metadata.GetTableName() ?? e.Metadata.ClrType.Name,
                State = e.State
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (trackedChanges.Count > 0)
        {
            foreach (var c in trackedChanges)
            {
                PendingChanges.Add(new PendingChange
                {
                    TableName = c.Table,
                    RecordId = c.Entity.Id,
                    ChangeType = c.State.ToString(),
                    ChangedAt = DateTime.UtcNow
                });
            }
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
