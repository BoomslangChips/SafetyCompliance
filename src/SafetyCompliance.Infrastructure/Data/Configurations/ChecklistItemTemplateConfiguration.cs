using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class ChecklistItemTemplateConfiguration : IEntityTypeConfiguration<ChecklistItemTemplate>
{
    public void Configure(EntityTypeBuilder<ChecklistItemTemplate> builder)
    {
        builder.ToTable("ChecklistItemTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ItemName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.EquipmentTypeId);
        builder.HasIndex(x => x.EquipmentSubTypeId);

        builder.HasOne(x => x.EquipmentSubType)
               .WithMany()
               .HasForeignKey(x => x.EquipmentSubTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
