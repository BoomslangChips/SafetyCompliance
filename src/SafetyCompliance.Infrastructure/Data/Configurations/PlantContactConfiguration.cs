using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class PlantContactConfiguration : IEntityTypeConfiguration<PlantContact>
{
    public void Configure(EntityTypeBuilder<PlantContact> builder)
    {
        builder.ToTable("PlantContacts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Role).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => x.PlantId);
        builder.HasIndex(x => new { x.PlantId, x.Category });

        builder.HasOne(x => x.Plant)
               .WithMany(x => x.Contacts)
               .HasForeignKey(x => x.PlantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
