using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface ISetupService
{
    Task<int> ExecuteSetupAsync(SetupCreateDto dto, string userId, CancellationToken ct = default);
}
