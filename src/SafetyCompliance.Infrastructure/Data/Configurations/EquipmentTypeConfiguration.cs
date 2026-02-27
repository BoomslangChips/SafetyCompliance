using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class EquipmentTypeConfiguration : IEntityTypeConfiguration<EquipmentType>
{
    public void Configure(EntityTypeBuilder<EquipmentType> builder)
    {
        builder.ToTable("EquipmentTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IconClass).HasMaxLength(100);
        builder.HasMany(x => x.ChecklistItemTemplates).WithOne(x => x.EquipmentType).HasForeignKey(x => x.EquipmentTypeId);
        builder.HasMany(x => x.SubTypes).WithOne(x => x.EquipmentType).HasForeignKey(x => x.EquipmentTypeId);
        builder.HasMany(x => x.Equipment).WithOne(x => x.EquipmentType).HasForeignKey(x => x.EquipmentTypeId);
    }
}
