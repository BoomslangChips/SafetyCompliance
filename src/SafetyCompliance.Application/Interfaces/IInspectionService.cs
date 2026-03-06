using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IInspectionService
{
    Task<List<InspectionRoundDto>> GetInspectionRoundsAsync(int plantId, CancellationToken ct = default);
    Task<InspectionRoundDto> StartInspectionRoundAsync(int plantId, string userId, int? scheduleId = null, CancellationToken ct = default);
    Task<List<EquipmentInspectionDto>> GetEquipmentInspectionsAsync(int roundId, CancellationToken ct = default);
    Task SubmitResponseAsync(SubmitResponseDto dto, CancellationToken ct = default);
    Task SaveEquipmentCommentAsync(int equipmentInspectionId, string? comments, CancellationToken ct = default);
    Task CompleteRoundAsync(int roundId, CancellationToken ct = default);
    Task<int> UploadPhotoAsync(int equipmentInspectionId, string fileName, string filePath, string contentType, long fileSize, string userId, CancellationToken ct = default);
    Task<List<InspectionPhotoDto>> GetPhotosAsync(int equipmentInspectionId, CancellationToken ct = default);
    Task<Dictionary<int, List<InspectionPhotoDto>>> GetPhotosByRoundAsync(int roundId, CancellationToken ct = default);
    Task<string?> DeletePhotoAsync(int photoId, CancellationToken ct = default);
    Task<List<InspectionRoundDto>> GetActiveRoundsAsync(CancellationToken ct = default);
    Task<List<FailedInspectionItemDto>> GetFailedItemsAsync(CancellationToken ct = default);
    Task DeleteRoundAsync(int roundId, CancellationToken ct = default);
}
