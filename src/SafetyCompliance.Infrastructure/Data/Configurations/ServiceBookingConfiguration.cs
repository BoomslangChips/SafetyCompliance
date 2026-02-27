using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class ServiceBookingConfiguration : IEntityTypeConfiguration<ServiceBooking>
{
    public void Configure(EntityTypeBuilder<ServiceBooking> builder)
    {
        builder.ToTable("ServiceBookings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ServiceProvider).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.CreatedById).HasMaxLength(450);
        builder.Property(x => x.ModifiedById).HasMaxLength(450);
        builder.HasIndex(x => x.EquipmentId);
        builder.HasIndex(x => x.Status);
        builder.HasOne(x => x.Equipment).WithMany(e => e.ServiceBookings).HasForeignKey(x => x.EquipmentId);
        builder.HasOne(x => x.EquipmentInspection).WithMany().HasForeignKey(x => x.EquipmentInspectionId).IsRequired(false);
    }
}
