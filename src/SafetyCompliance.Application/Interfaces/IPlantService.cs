using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IPlantService
{
    Task<List<PlantDto>> GetAllPlantsAsync(CancellationToken ct = default);
    Task<List<PlantDto>> GetPlantsByCompanyAsync(int companyId, CancellationToken ct = default);
    Task<PlantDto?> GetPlantByIdAsync(int id, CancellationToken ct = default);
    Task<PlantDto> CreatePlantAsync(PlantCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdatePlantAsync(PlantUpdateDto dto, string userId, CancellationToken ct = default);

    // ── Contacts ──────────────────────────────────────────────────────────
    Task<Dictionary<int, int>> GetContactCountsByPlantsAsync(IEnumerable<int> plantIds, CancellationToken ct = default);
    Task<List<PlantContactDto>> GetContactsAsync(int plantId, CancellationToken ct = default);
    Task<PlantContactDto> AddContactAsync(PlantContactCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateContactAsync(PlantContactUpdateDto dto, string userId, CancellationToken ct = default);
    Task DeleteContactAsync(int contactId, CancellationToken ct = default);

    // ── Contact Documents ────────────────────────────────────────────────
    Task<List<ContactDocumentDto>> GetContactDocumentsAsync(int contactId, CancellationToken ct = default);
    Task<ContactDocumentDto> UploadContactDocumentAsync(ContactDocumentUploadDto dto, string userId, CancellationToken ct = default);
    Task RenameContactDocumentAsync(int documentId, string newDisplayName, string userId, CancellationToken ct = default);
    Task DeleteContactDocumentAsync(int documentId, CancellationToken ct = default);
    Task<(string FileBase64, string ContentType, string FileName)?> DownloadContactDocumentAsync(int documentId, CancellationToken ct = default);
}
