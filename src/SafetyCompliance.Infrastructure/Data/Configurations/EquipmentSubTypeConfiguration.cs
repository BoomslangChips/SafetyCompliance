using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class EquipmentSubTypeConfiguration : IEntityTypeConfiguration<EquipmentSubType>
{
    public void Configure(EntityTypeBuilder<EquipmentSubType> builder)
    {
        builder.ToTable("EquipmentSubTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}
