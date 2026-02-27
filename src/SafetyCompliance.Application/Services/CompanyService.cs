using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class CompanyService(ApplicationDbContext context) : ICompanyService
{
    public async Task<List<CompanyDto>> GetAllCompaniesAsync(CancellationToken ct = default)
    {
        return await context.Companies
            .Where(c => c.IsActive)
            .Select(c => new CompanyDto(
                c.Id, c.Name, c.Code, c.Address, c.ContactName, c.ContactEmail, c.ContactPhone, c.IsActive,
                c.Plants.Count(p => p.IsActive),
                c.Plants.Where(p => p.IsActive)
                    .SelectMany(p => p.Sections.Where(s => s.IsActive))
                    .SelectMany(s => s.Equipment.Where(e => e.IsActive)).Count()))
            .ToListAsync(ct);
    }

    public async Task<CompanyDto?> GetCompanyByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Companies
            .Where(c => c.Id == id)
            .Select(c => new CompanyDto(
                c.Id, c.Name, c.Code, c.Address, c.ContactName, c.ContactEmail, c.ContactPhone, c.IsActive,
                c.Plants.Count(p => p.IsActive),
                c.Plants.Where(p => p.IsActive)
                    .SelectMany(p => p.Sections.Where(s => s.IsActive))
                    .SelectMany(s => s.Equipment.Where(e => e.IsActive)).Count()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CompanyDto> CreateCompanyAsync(CompanyCreateDto dto, string userId, CancellationToken ct = default)
    {
        var company = new Company
        {
            Name = dto.Name,
            Code = dto.Code,
            Address = dto.Address,
            ContactName = dto.ContactName,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            CreatedById = userId
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync(ct);

        return new CompanyDto(company.Id, company.Name, company.Code, company.Address, company.ContactName, company.ContactEmail, company.ContactPhone, company.IsActive, 0, 0);
    }

    public async Task UpdateCompanyAsync(CompanyUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var company = await context.Companies.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Company {dto.Id} not found");

        company.Name = dto.Name;
        company.Code = dto.Code;
        company.Address = dto.Address;
        company.ContactName = dto.ContactName;
        company.ContactEmail = dto.ContactEmail;
        company.ContactPhone = dto.ContactPhone;
        company.IsActive = dto.IsActive;
        company.ModifiedAt = DateTime.UtcNow;
        company.ModifiedById = userId;

        await context.SaveChangesAsync(ct);
    }
}
