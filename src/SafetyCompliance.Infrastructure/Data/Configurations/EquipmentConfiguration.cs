using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Identifier).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(300);
        builder.Property(x => x.Size).HasMaxLength(50);
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.HasIndex(x => x.SectionId);
        builder.HasIndex(x => x.EquipmentTypeId);
        builder.HasOne(x => x.EquipmentSubType).WithMany().HasForeignKey(x => x.EquipmentSubTypeId).IsRequired(false);
        builder.HasMany(x => x.EquipmentInspections).WithOne(x => x.Equipment).HasForeignKey(x => x.EquipmentId);
    }
}
