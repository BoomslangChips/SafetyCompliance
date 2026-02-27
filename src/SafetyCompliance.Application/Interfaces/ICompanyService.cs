using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllCompaniesAsync(CancellationToken ct = default);
    Task<CompanyDto?> GetCompanyByIdAsync(int id, CancellationToken ct = default);
    Task<CompanyDto> CreateCompanyAsync(CompanyCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateCompanyAsync(CompanyUpdateDto dto, string userId, CancellationToken ct = default);
    Task<List<HierarchyCompanyDto>> GetHierarchyAsync(CancellationToken ct = default);
}
