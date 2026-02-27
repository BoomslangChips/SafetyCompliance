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
}
