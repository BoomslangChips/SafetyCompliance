using SafetyCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SafetyCompliance.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.ContactName).HasMaxLength(200);
        builder.Property(x => x.ContactEmail).HasMaxLength(200);
        builder.Property(x => x.ContactPhone).HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        builder.HasMany(x => x.Plants).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId);
        builder.HasMany(x => x.UserCompanies).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId);
    }
}
