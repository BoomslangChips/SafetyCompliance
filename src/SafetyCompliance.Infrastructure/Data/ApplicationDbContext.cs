using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Domain.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<EquipmentType> EquipmentTypes => Set<EquipmentType>();
    public DbSet<EquipmentSubType> EquipmentSubTypes => Set<EquipmentSubType>();
    public DbSet<ChecklistItemTemplate> ChecklistItemTemplates => Set<ChecklistItemTemplate>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<InspectionRound> InspectionRounds => Set<InspectionRound>();
    public DbSet<EquipmentInspection> EquipmentInspections => Set<EquipmentInspection>();
    public DbSet<InspectionResponse> InspectionResponses => Set<InspectionResponse>();
    public DbSet<InspectionPhoto> InspectionPhotos => Set<InspectionPhoto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
