using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.DTOs;

public record NoteDto(
    int          Id,
    string       Title,
    string       Content,
    NoteCategory Category,
    NotePriority Priority,
    bool         IsPinned,
    int?         EquipmentId,
    string?      EquipmentIdentifier,
    string?      EquipmentTypeName,
    int?         CompanyId,
    string?      CompanyName,
    int?         PlantId,
    string?      PlantName,
    string?      SectionName,
    string       CreatedById,
    DateTime     CreatedAt,
    DateTime?    ModifiedAt);

public record NoteCreateDto(
    string       Title,
    string       Content,
    NoteCategory Category,
    NotePriority Priority,
    bool         IsPinned,
    int?         EquipmentId,
    int?         CompanyId,
    int?         PlantId);

public record NoteUpdateDto(
    int          Id,
    string       Title,
    string       Content,
    NoteCategory Category,
    NotePriority Priority,
    bool         IsPinned);
