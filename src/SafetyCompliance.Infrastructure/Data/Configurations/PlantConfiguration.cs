using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.ToTable("Plants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ContactName).HasMaxLength(200);
        builder.Property(x => x.ContactPhone).HasMaxLength(50);
        builder.Property(x => x.ContactEmail).HasMaxLength(200);
        builder.HasIndex(x => x.CompanyId);
        builder.HasMany(x => x.Sections).WithOne(x => x.Plant).HasForeignKey(x => x.PlantId);
        builder.HasMany(x => x.InspectionRounds).WithOne(x => x.Plant).HasForeignKey(x => x.PlantId);
    }
}
