using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IInspectionService
{
    Task<List<InspectionRoundDto>> GetInspectionRoundsAsync(int plantId, CancellationToken ct = default);
    Task<InspectionRoundDto> StartInspectionRoundAsync(int plantId, string userId, CancellationToken ct = default);
    Task<List<EquipmentInspectionDto>> GetEquipmentInspectionsAsync(int roundId, CancellationToken ct = default);
    Task SubmitResponseAsync(SubmitResponseDto dto, CancellationToken ct = default);
    Task CompleteRoundAsync(int roundId, CancellationToken ct = default);
    Task<int> UploadPhotoAsync(int equipmentInspectionId, string fileName, string filePath, string contentType, long fileSize, string userId, CancellationToken ct = default);
}
