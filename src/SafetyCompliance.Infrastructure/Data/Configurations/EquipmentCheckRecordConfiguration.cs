using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class EquipmentCheckRecordConfiguration : IEntityTypeConfiguration<EquipmentCheckRecord>
{
    public void Configure(EntityTypeBuilder<EquipmentCheckRecord> builder)
    {
        builder.ToTable("EquipmentCheckRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => x.EquipmentId);
        builder.HasIndex(x => x.EquipmentCheckId);
        builder.HasIndex(x => new { x.EquipmentId, x.EquipmentCheckId }).IsUnique();

        builder.HasOne(x => x.EquipmentCheck)
               .WithMany(x => x.CheckRecords)
               .HasForeignKey(x => x.EquipmentCheckId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
