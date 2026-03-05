using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class EquipmentCheckConfiguration : IEntityTypeConfiguration<EquipmentCheck>
{
    public void Configure(EntityTypeBuilder<EquipmentCheck> builder)
    {
        builder.ToTable("EquipmentChecks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.EquipmentTypeId);
        builder.HasIndex(x => x.EquipmentSubTypeId);

        builder.HasOne(x => x.EquipmentSubType)
               .WithMany()
               .HasForeignKey(x => x.EquipmentSubTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
