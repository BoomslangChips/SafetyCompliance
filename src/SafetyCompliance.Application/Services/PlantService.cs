using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class PlantService(ApplicationDbContext context) : IPlantService
{
    public async Task<List<PlantDto>> GetPlantsByCompanyAsync(int companyId, CancellationToken ct = default)
    {
        return await context.Plants
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .Select(p => new PlantDto(
                p.Id, p.CompanyId, p.Company.Name, p.Name, p.Description, p.ContactName, p.ContactPhone, p.ContactEmail, p.IsActive,
                p.Sections.Count(s => s.IsActive),
                p.Sections.Where(s => s.IsActive).SelectMany(s => s.Equipment.Where(e => e.IsActive)).Count(),
                p.PhotoBase64, p.PhotoFileName))
            .ToListAsync(ct);
    }

    public async Task<PlantDto?> GetPlantByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Plants
            .Where(p => p.Id == id)
            .Select(p => new PlantDto(
                p.Id, p.CompanyId, p.Company.Name, p.Name, p.Description, p.ContactName, p.ContactPhone, p.ContactEmail, p.IsActive,
                p.Sections.Count(s => s.IsActive),
                p.Sections.Where(s => s.IsActive).SelectMany(s => s.Equipment.Where(e => e.IsActive)).Count(),
                p.PhotoBase64, p.PhotoFileName))
            .FirstOrDefaultAsync(ct);
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
            plant.PhotoBase64 = dto.PhotoBase64;
            plant.PhotoFileName = dto.PhotoFileName;
        }
        plant.ModifiedAt = DateTime.UtcNow;
        plant.ModifiedById = userId;

        await context.SaveChangesAsync(ct);
    }

    // ── Plant Contacts ────────────────────────────────────────────────────

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
}
