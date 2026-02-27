using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface ISectionService
{
    Task<List<SectionDto>> GetSectionsByPlantAsync(int plantId, CancellationToken ct = default);
    Task<SectionDto?> GetSectionByIdAsync(int id, CancellationToken ct = default);
    Task<SectionDto> CreateSectionAsync(SectionCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateSectionAsync(SectionUpdateDto dto, string userId, CancellationToken ct = default);
}
