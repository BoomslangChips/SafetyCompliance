using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.Interfaces;

public interface INoteService
{
    Task<List<NoteDto>> GetNotesAsync(
        int?         equipmentId = null,
        int?         plantId     = null,
        int?         companyId   = null,
        NoteCategory? category   = null,
        CancellationToken ct     = default);

    Task<NoteDto?> GetNoteByIdAsync(int id, CancellationToken ct = default);

    Task<NoteDto> CreateNoteAsync(NoteCreateDto dto, string userId, CancellationToken ct = default);

    Task UpdateNoteAsync(NoteUpdateDto dto, string userId, CancellationToken ct = default);

    Task TogglePinnedAsync(int id, string userId, CancellationToken ct = default);

    Task DeleteNoteAsync(int id, CancellationToken ct = default);
}
