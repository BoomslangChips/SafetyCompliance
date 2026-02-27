using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IPlantService
{
    Task<List<PlantDto>> GetPlantsByCompanyAsync(int companyId, CancellationToken ct = default);
    Task<PlantDto?> GetPlantByIdAsync(int id, CancellationToken ct = default);
    Task<PlantDto> CreatePlantAsync(PlantCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdatePlantAsync(PlantUpdateDto dto, string userId, CancellationToken ct = default);
}
