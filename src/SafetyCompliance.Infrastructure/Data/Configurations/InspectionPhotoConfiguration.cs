using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class InspectionPhotoConfiguration : IEntityTypeConfiguration<InspectionPhoto>
{
    public void Configure(EntityTypeBuilder<InspectionPhoto> builder)
    {
        builder.ToTable("InspectionPhotos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Caption).HasMaxLength(300);
        builder.HasIndex(x => x.EquipmentInspectionId);
        builder.Property(x => x.UploadedById).HasMaxLength(450);
    }
}
