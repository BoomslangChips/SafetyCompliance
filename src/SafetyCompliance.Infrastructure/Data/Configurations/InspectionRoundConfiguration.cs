using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class InspectionRoundConfiguration : IEntityTypeConfiguration<InspectionRound>
{
    public void Configure(EntityTypeBuilder<InspectionRound> builder)
    {
        builder.ToTable("InspectionRounds");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InspectionMonth).HasMaxLength(7);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.HasIndex(x => new { x.PlantId, x.InspectionDate });
        builder.HasIndex(x => new { x.PlantId, x.InspectionMonth }).IsUnique();
        builder.HasOne(x => x.InspectedBy).WithMany().HasForeignKey(x => x.InspectedById).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ReviewedBy).WithMany().HasForeignKey(x => x.ReviewedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(x => x.EquipmentInspections).WithOne(x => x.InspectionRound).HasForeignKey(x => x.InspectionRoundId).OnDelete(DeleteBehavior.Cascade);
    }
}
