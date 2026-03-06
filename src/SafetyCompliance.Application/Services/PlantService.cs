using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class PlantService(ApplicationDbContext context) : IPlantService
{
    public async Task<List<PlantDto>> GetAllPlantsAsync(CancellationToken ct = default)
    {
        var plants = await context.Plants
            .Where(p => p.IsActive)
            .OrderBy(p => p.Company.Name).ThenBy(p => p.Name)
            .Select(p => new PlantDto(
                p.Id, p.CompanyId, p.Company.Name, p.Name, p.Description,
                p.ContactName, p.ContactPhone, p.ContactEmail, p.IsActive,
                0, 0, null, null))
            .ToListAsync(ct);
        return plants;
    }

    public async Task<List<PlantDto>> GetPlantsByCompanyAsync(int companyId, CancellationToken ct = default)
    {
        var plants = await context.Plants
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .Select(p => new
            {
                p.Id, p.CompanyId, CompanyName = p.Company.Name,
                p.Name, p.Description, p.ContactName, p.ContactPhone, p.ContactEmail,
                p.IsActive, p.PhotoBase64, p.PhotoFileName
            })
            .ToListAsync(ct);

        if (plants.Count == 0) return [];

        var plantIds = plants.Select(p => p.Id).ToList();

        var sectionCounts = await context.Sections
            .Where(s => s.IsActive && plantIds.Contains(s.PlantId))
            .GroupBy(s => s.PlantId)
            .Select(g => new { PlantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.PlantId, g => g.Count, ct);

        var equipCounts = await context.Equipment
            .Where(e => e.IsActive && e.SectionId != null && e.Section!.IsActive && plantIds.Contains(e.Section.PlantId))
            .GroupBy(e => e.Section!.PlantId)
            .Select(g => new { PlantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.PlantId, g => g.Count, ct);

        return plants.Select(p => new PlantDto(
            p.Id, p.CompanyId, p.CompanyName, p.Name, p.Description,
            p.ContactName, p.ContactPhone, p.ContactEmail, p.IsActive,
            sectionCounts.GetValueOrDefault(p.Id),
            equipCounts.GetValueOrDefault(p.Id),
            p.PhotoBase64, p.PhotoFileName)).ToList();
    }

    public async Task<PlantDto?> GetPlantByIdAsync(int id, CancellationToken ct = default)
    {
        var plant = await context.Plants
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id, p.CompanyId, CompanyName = p.Company.Name,
                p.Name, p.Description, p.ContactName, p.ContactPhone, p.ContactEmail,
                p.IsActive, p.PhotoBase64, p.PhotoFileName
            })
            .FirstOrDefaultAsync(ct);

        if (plant is null) return null;

        var sectionCount = await context.Sections
            .CountAsync(s => s.PlantId == id && s.IsActive, ct);

        var equipCount = await context.Equipment
            .CountAsync(e => e.IsActive && e.SectionId != null && e.Section!.IsActive && e.Section.PlantId == id, ct);

        return new PlantDto(
            plant.Id, plant.CompanyId, plant.CompanyName, plant.Name, plant.Description,
            plant.ContactName, plant.ContactPhone, plant.ContactEmail, plant.IsActive,
            sectionCount, equipCount,
            plant.PhotoBase64, plant.PhotoFileName);
    }

    public async Task<PlantDto> CreatePlantAsync(PlantCreateDto dto, string userId, CancellationToken ct = default)
    {
        var plant = new Plant
        {
            CompanyId = dto.CompanyId,
            Name = dto.Name,
            Description = dto.Description,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            PhotoBase64 = dto.PhotoBase64,
            PhotoFileName = dto.PhotoFileName,
            CreatedById = userId
        };

        context.Plants.Add(plant);
        await context.SaveChangesAsync(ct);

        return (await GetPlantByIdAsync(plant.Id, ct))!;
    }

    public async Task UpdatePlantAsync(PlantUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var plant = await context.Plants.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Plant {dto.Id} not found");

        plant.Name = dto.Name;
        plant.Description = dto.Description;
        plant.ContactName = dto.ContactName;
        plant.ContactPhone = dto.ContactPhone;
        plant.ContactEmail = dto.ContactEmail;
        plant.IsActive = dto.IsActive;
        if (dto.PhotoBase64 is not null)
        {
            plant.PhotoBase64 = dto.PhotoBase64 == "" ? null : dto.PhotoBase64;
            plant.PhotoFileName = dto.PhotoBase64 == "" ? null : dto.PhotoFileName;
        }
        plant.ModifiedAt = DateTime.UtcNow;
        plant.ModifiedById = userId;

        await context.SaveChangesAsync(ct);
    }

    // ── Plant Contacts ────────────────────────────────────────────────────

    public async Task<Dictionary<int, int>> GetContactCountsByPlantsAsync(IEnumerable<int> plantIds, CancellationToken ct = default)
    {
        var ids = plantIds.ToList();
        return await context.PlantContacts
            .Where(c => ids.Contains(c.PlantId))
            .GroupBy(c => c.PlantId)
            .Select(g => new { PlantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.PlantId, g => g.Count, ct);
    }

    public async Task<List<PlantContactDto>> GetContactsAsync(int plantId, CancellationToken ct = default)
    {
        return await context.PlantContacts
            .Where(c => c.PlantId == plantId)
            .OrderBy(c => c.Category)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new PlantContactDto(
                c.Id, c.PlantId, c.Category, c.Name, c.Role,
                c.Phone, c.Email, c.Notes, c.IsPrimary, c.SortOrder))
            .ToListAsync(ct);
    }

    public async Task<PlantContactDto> AddContactAsync(PlantContactCreateDto dto, string userId, CancellationToken ct = default)
    {
        var maxOrder = await context.PlantContacts
            .Where(c => c.PlantId == dto.PlantId && c.Category == dto.Category)
            .Select(c => (int?)c.SortOrder)
            .MaxAsync(ct) ?? 0;

        var contact = new PlantContact
        {
            PlantId   = dto.PlantId,
            Category  = dto.Category.Trim(),
            Name      = dto.Name.Trim(),
            Role      = dto.Role?.Trim(),
            Phone     = dto.Phone?.Trim(),
            Email     = dto.Email?.Trim(),
            Notes     = dto.Notes?.Trim(),
            IsPrimary = dto.IsPrimary,
            SortOrder = maxOrder + 1,
            CreatedById = userId
        };

        context.PlantContacts.Add(contact);
        await context.SaveChangesAsync(ct);

        return new PlantContactDto(contact.Id, contact.PlantId, contact.Category, contact.Name,
            contact.Role, contact.Phone, contact.Email, contact.Notes, contact.IsPrimary, contact.SortOrder);
    }

    public async Task UpdateContactAsync(PlantContactUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var contact = await context.PlantContacts.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"PlantContact {dto.Id} not found");

        contact.Category  = dto.Category.Trim();
        contact.Name      = dto.Name.Trim();
        contact.Role      = dto.Role?.Trim();
        contact.Phone     = dto.Phone?.Trim();
        contact.Email     = dto.Email?.Trim();
        contact.Notes     = dto.Notes?.Trim();
        contact.IsPrimary = dto.IsPrimary;
        contact.ModifiedAt   = DateTime.UtcNow;
        contact.ModifiedById = userId;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteContactAsync(int contactId, CancellationToken ct = default)
    {
        var contact = await context.PlantContacts.FindAsync([contactId], ct);
        if (contact is not null)
        {
            context.PlantContacts.Remove(contact);
            await context.SaveChangesAsync(ct);
        }
    }

    // ── Contact Documents ──────────────────────────────────────────────────

    public async Task<List<ContactDocumentDto>> GetContactDocumentsAsync(int contactId, CancellationToken ct = default)
    {
        return await context.ContactDocuments
            .Where(d => d.PlantContactId == contactId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new ContactDocumentDto(
                d.Id, d.PlantContactId, d.FileName, d.DisplayName,
                d.ContentType, d.FileSizeBytes, d.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<ContactDocumentDto> UploadContactDocumentAsync(ContactDocumentUploadDto dto, string userId, CancellationToken ct = default)
    {
        var doc = new ContactDocument
        {
            PlantContactId = dto.PlantContactId,
            FileName       = dto.FileName,
            DisplayName    = dto.DisplayName,
            ContentType    = dto.ContentType,
            FileSizeBytes  = dto.FileSizeBytes,
            FileBase64     = dto.FileBase64,
            CreatedById    = userId
        };

        context.ContactDocuments.Add(doc);
        await context.SaveChangesAsync(ct);

        return new ContactDocumentDto(doc.Id, doc.PlantContactId, doc.FileName, doc.DisplayName,
            doc.ContentType, doc.FileSizeBytes, doc.CreatedAt);
    }

    public async Task RenameContactDocumentAsync(int documentId, string newDisplayName, string userId, CancellationToken ct = default)
    {
        var doc = await context.ContactDocuments.FindAsync([documentId], ct)
            ?? throw new InvalidOperationException($"ContactDocument {documentId} not found");

        doc.DisplayName  = newDisplayName.Trim();
        doc.ModifiedAt   = DateTime.UtcNow;
        doc.ModifiedById = userId;
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteContactDocumentAsync(int documentId, CancellationToken ct = default)
    {
        var doc = await context.ContactDocuments.FindAsync([documentId], ct);
        if (doc is not null)
        {
            context.ContactDocuments.Remove(doc);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<(string FileBase64, string ContentType, string FileName)?> DownloadContactDocumentAsync(int documentId, CancellationToken ct = default)
    {
        var doc = await context.ContactDocuments
            .Where(d => d.Id == documentId)
            .Select(d => new { d.FileBase64, d.ContentType, d.DisplayName })
            .FirstOrDefaultAsync(ct);

        if (doc is null) return null;
        return (doc.FileBase64, doc.ContentType, doc.DisplayName);
    }
}
