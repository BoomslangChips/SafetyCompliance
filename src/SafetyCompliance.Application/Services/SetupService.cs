using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;

namespace SafetyCompliance.Application.Services;

public class SetupService(ApplicationDbContext context) : ISetupService
{
    public async Task<int> ExecuteSetupAsync(SetupCreateDto dto, string userId, CancellationToken ct = default)
    {
        int companyId;

        if (dto.CompanyId.HasValue)
        {
            companyId = dto.CompanyId.Value;
        }
        else
        {
            var company = new Company
            {
                Name = dto.CompanyName ?? "New Company",
                Code = dto.CompanyCode,
                Address = dto.CompanyAddress,
                ContactName = dto.CompanyContactName,
                ContactEmail = dto.CompanyContactEmail,
                ContactPhone = dto.CompanyContactPhone,
                PhotoBase64 = dto.CompanyPhotoBase64,
                PhotoFileName = dto.CompanyPhotoFileName,
                CreatedById = userId
            };
            context.Companies.Add(company);
            await context.SaveChangesAsync(ct);
            companyId = company.Id;
        }

        foreach (var plantItem in dto.Plants)
        {
            var plant = new Plant
            {
                CompanyId = companyId,
                Name = plantItem.Name,
                Description = plantItem.Description,
                ContactName = plantItem.ContactName,
                ContactPhone = plantItem.ContactPhone,
                ContactEmail = plantItem.ContactEmail,
                PhotoBase64 = plantItem.PhotoBase64,
                PhotoFileName = plantItem.PhotoFileName,
                CreatedById = userId
            };
            context.Plants.Add(plant);
            await context.SaveChangesAsync(ct);

            var sectionOrder = 1;
            foreach (var sectionItem in plantItem.Sections)
            {
                var section = new Section
                {
                    PlantId = plant.Id,
                    Name = sectionItem.Name,
                    Description = sectionItem.Description,
                    SortOrder = sectionOrder++,
                    PhotoBase64 = sectionItem.PhotoBase64,
                    PhotoFileName = sectionItem.PhotoFileName,
                    CreatedById = userId
                };
                context.Sections.Add(section);
                await context.SaveChangesAsync(ct);

                var equipOrder = 1;
                foreach (var eqItem in sectionItem.Equipment)
                {
                    var equipment = new Equipment
                    {
                        SectionId = section.Id,
                        EquipmentTypeId = eqItem.EquipmentTypeId,
                        EquipmentSubTypeId = eqItem.EquipmentSubTypeId,
                        Identifier = eqItem.Identifier,
                        Description = eqItem.Description,
                        Size = eqItem.Size,
                        SerialNumber = eqItem.SerialNumber,
                        SortOrder = equipOrder++,
                        CreatedById = userId
                    };
                    context.Equipment.Add(equipment);
                }
                await context.SaveChangesAsync(ct);
            }
        }

        return companyId;
    }
}
