using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class ContactDocumentConfiguration : IEntityTypeConfiguration<ContactDocument>
{
    public void Configure(EntityTypeBuilder<ContactDocument> builder)
    {
        builder.ToTable("ContactDocuments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileBase64).IsRequired();

        builder.HasIndex(x => x.PlantContactId);

        builder.HasOne(x => x.PlantContact)
               .WithMany(x => x.Documents)
               .HasForeignKey(x => x.PlantContactId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
