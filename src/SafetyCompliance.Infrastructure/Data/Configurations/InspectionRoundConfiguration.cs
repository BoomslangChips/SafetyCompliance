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
        builder.Property(x => x.InspectedById).HasMaxLength(450);
        builder.Property(x => x.ReviewedById).HasMaxLength(450);
        builder.HasMany(x => x.EquipmentInspections).WithOne(x => x.InspectionRound).HasForeignKey(x => x.InspectionRoundId).OnDelete(DeleteBehavior.Cascade);
    }
}
