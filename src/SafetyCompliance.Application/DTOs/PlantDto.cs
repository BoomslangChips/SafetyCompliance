namespace SafetyCompliance.Application.DTOs;

public record PlantDto(int Id, int CompanyId, string CompanyName, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail, bool IsActive, int SectionCount, int EquipmentCount, string? PhotoBase64 = null, string? PhotoFileName = null);
public record PlantCreateDto(int CompanyId, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail, string? PhotoBase64 = null, string? PhotoFileName = null);
public record PlantUpdateDto(int Id, string Name, string? Description, string? ContactName, string? ContactPhone, string? ContactEmail, bool IsActive, string? PhotoBase64 = null, string? PhotoFileName = null);

// ── Plant Contacts ──────────────────────────────────────────────────────────
public record PlantContactDto(
    int Id,
    int PlantId,
    string Category,
    string Name,
    string? Role,
    string? Phone,
    string? Email,
    string? Notes,
    bool IsPrimary,
    int SortOrder);

public record PlantContactCreateDto(
    int PlantId,
    string Category,
    string Name,
    string? Role,
    string? Phone,
    string? Email,
    string? Notes,
    bool IsPrimary);

public record PlantContactUpdateDto(
    int Id,
    string Category,
    string Name,
    string? Role,
    string? Phone,
    string? Email,
    string? Notes,
    bool IsPrimary);

// ── Contact Documents ───────────────────────────────────────────────────────
public record ContactDocumentDto(
    int Id,
    int PlantContactId,
    string FileName,
    string DisplayName,
    string ContentType,
    long FileSizeBytes,
    DateTime UploadedAt);

public record ContactDocumentUploadDto(
    int PlantContactId,
    string FileName,
    string DisplayName,
    string ContentType,
    long FileSizeBytes,
    string FileBase64);
