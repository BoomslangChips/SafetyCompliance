using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class CompanyService(ApplicationDbContext context) : ICompanyService
{
    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a plant-count dictionary and an equipment-count dictionary for
    /// a set of companies, using three flat queries instead of correlated
    /// sub-queries (which caused 30-second timeouts on unindexed tables).
    /// </summary>
    private async Task<(Dictionary<int, int> plantCounts, Dictionary<int, int> equipCounts)>
        GetCountsForCompaniesAsync(IEnumerable<int> companyIds, CancellationToken ct)
    {
        var ids = companyIds.ToList();

        var plantCounts = await context.Plants
            .Where(p => p.IsActive && ids.Contains(p.CompanyId))
            .GroupBy(p => p.CompanyId)
            .Select(g => new { CompanyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CompanyId, g => g.Count, ct);

        var equipCounts = await context.Equipment
            .Where(e => e.IsActive
                     && e.SectionId != null
                     && e.Section!.IsActive
                     && e.Section.Plant.IsActive
                     && ids.Contains(e.Section.Plant.CompanyId))
            .GroupBy(e => e.Section!.Plant.CompanyId)
            .Select(g => new { CompanyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CompanyId, g => g.Count, ct);

        return (plantCounts, equipCounts);
    }

    // ── queries ──────────────────────────────────────────────────────────────

    public async Task<List<CompanyDto>> GetAllCompaniesAsync(CancellationToken ct = default)
    {
        var companies = await context.Companies
            .Where(c => c.IsActive)
            .Select(c => new
            {
                c.Id, c.Name, c.Code, c.Address,
                c.ContactName, c.ContactEmail, c.ContactPhone,
                c.IsActive, c.PhotoBase64, c.PhotoFileName
            })
            .ToListAsync(ct);

        if (companies.Count == 0) return [];

        var (plantCounts, equipCounts) =
            await GetCountsForCompaniesAsync(companies.Select(c => c.Id), ct);

        return companies.Select(c => new CompanyDto(
            c.Id, c.Name, c.Code, c.Address,
            c.ContactName, c.ContactEmail, c.ContactPhone, c.IsActive,
            plantCounts.GetValueOrDefault(c.Id),
            equipCounts.GetValueOrDefault(c.Id),
            c.PhotoBase64, c.PhotoFileName)).ToList();
    }

    public async Task<CompanyDto?> GetCompanyByIdAsync(int id, CancellationToken ct = default)
    {
        var company = await context.Companies
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.Name, c.Code, c.Address,
                c.ContactName, c.ContactEmail, c.ContactPhone,
                c.IsActive, c.PhotoBase64, c.PhotoFileName
            })
            .FirstOrDefaultAsync(ct);

        if (company is null) return null;

        var plantCount = await context.Plants
            .CountAsync(p => p.CompanyId == id && p.IsActive, ct);

        var equipCount = await context.Equipment
            .CountAsync(e => e.IsActive
                          && e.SectionId != null
                          && e.Section!.IsActive
                          && e.Section.Plant.IsActive
                          && e.Section.Plant.CompanyId == id, ct);

        return new CompanyDto(
            company.Id, company.Name, company.Code, company.Address,
            company.ContactName, company.ContactEmail, company.ContactPhone, company.IsActive,
            plantCount, equipCount,
            company.PhotoBase64, company.PhotoFileName);
    }

    // ── mutations ─────────────────────────────────────────────────────────────

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
            PhotoBase64 = dto.PhotoBase64,
            PhotoFileName = dto.PhotoFileName,
            CreatedById = userId
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync(ct);

        return new CompanyDto(company.Id, company.Name, company.Code, company.Address,
            company.ContactName, company.ContactEmail, company.ContactPhone, company.IsActive,
            0, 0, company.PhotoBase64, company.PhotoFileName);
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
        if (dto.PhotoBase64 is not null)
        {
            // empty string = clear photo, non-empty = new photo
            company.PhotoBase64 = dto.PhotoBase64 == "" ? null : dto.PhotoBase64;
            company.PhotoFileName = dto.PhotoBase64 == "" ? null : dto.PhotoFileName;
        }
        company.ModifiedAt = DateTime.UtcNow;
        company.ModifiedById = userId;

        await context.SaveChangesAsync(ct);
    }

    public async Task<List<HierarchyCompanyDto>> GetHierarchyAsync(CancellationToken ct = default)
    {
        return await context.Companies
            .Where(c => c.IsActive)
            .Select(c => new HierarchyCompanyDto(
                c.Id,
                c.Name,
                c.Code,
                c.PhotoBase64,
                c.Address,
                c.ContactName,
                c.ContactEmail,
                c.Plants.Count(p => p.IsActive),
                c.Plants.Where(p => p.IsActive)
                    .SelectMany(p => p.Sections.Where(s => s.IsActive))
                    .SelectMany(s => s.Equipment.Where(e => e.IsActive)).Count(),
                c.Plants.Where(p => p.IsActive)
                    .SelectMany(p => p.Sections.Where(s => s.IsActive)).Count(),
                c.Plants.Where(p => p.IsActive).Select(p => new HierarchyPlantDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.PhotoBase64,
                    p.Sections.Count(s => s.IsActive),
                    p.Sections.Where(s => s.IsActive).SelectMany(s => s.Equipment.Where(e => e.IsActive)).Count(),
                    p.Sections.Where(s => s.IsActive).OrderBy(s => s.SortOrder).Select(s => new HierarchySectionDto(
                        s.Id,
                        s.Name,
                        s.Description,
                        s.PhotoBase64,
                        s.Equipment.Count(e => e.IsActive),
                        s.Equipment.Where(e => e.IsActive).OrderBy(e => e.SortOrder).Select(e => new HierarchyEquipmentDto(
                            e.Id,
                            e.Identifier,
                            e.EquipmentType.Name,
                            e.EquipmentSubType != null ? e.EquipmentSubType.Name : null
                        )).ToList()
                    )).ToList()
                )).ToList()
            ))
            .ToListAsync(ct);
    }
}
