using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class InspectionResponseConfiguration : IEntityTypeConfiguration<InspectionResponse>
{
    public void Configure(EntityTypeBuilder<InspectionResponse> builder)
    {
        builder.ToTable("InspectionResponses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comment).HasMaxLength(500);
        builder.HasIndex(x => new { x.EquipmentInspectionId, x.ChecklistItemTemplateId }).IsUnique();
        builder.HasOne(x => x.ChecklistItemTemplate).WithMany().HasForeignKey(x => x.ChecklistItemTemplateId).OnDelete(DeleteBehavior.NoAction);
    }
}
