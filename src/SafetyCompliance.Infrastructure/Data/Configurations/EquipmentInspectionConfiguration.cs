using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class EquipmentInspectionConfiguration : IEntityTypeConfiguration<EquipmentInspection>
{
    public void Configure(EntityTypeBuilder<EquipmentInspection> builder)
    {
        builder.ToTable("EquipmentInspections");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.InspectionRoundId);
        builder.HasIndex(x => new { x.InspectionRoundId, x.EquipmentId }).IsUnique();
        builder.HasMany(x => x.Responses).WithOne(x => x.EquipmentInspection).HasForeignKey(x => x.EquipmentInspectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Photos).WithOne(x => x.EquipmentInspection).HasForeignKey(x => x.EquipmentInspectionId).OnDelete(DeleteBehavior.Cascade);
    }
}
