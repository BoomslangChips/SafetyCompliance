using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Content)
               .HasMaxLength(4000)
               .IsRequired();

        builder.Property(x => x.Category)
               .HasConversion<int>();

        builder.Property(x => x.Priority)
               .HasConversion<int>();

        builder.Property(x => x.CreatedById)
               .HasMaxLength(450);

        builder.Property(x => x.ModifiedById)
               .HasMaxLength(450);

        builder.HasIndex(x => x.EquipmentId);
        builder.HasIndex(x => x.PlantId);
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.IsPinned);
        builder.HasIndex(x => x.Category);

        builder.HasOne(x => x.Equipment)
               .WithMany()
               .HasForeignKey(x => x.EquipmentId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Company)
               .WithMany()
               .HasForeignKey(x => x.CompanyId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Plant)
               .WithMany()
               .HasForeignKey(x => x.PlantId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
