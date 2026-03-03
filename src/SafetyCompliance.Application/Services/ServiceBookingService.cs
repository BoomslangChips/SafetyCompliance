using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class ServiceBookingService(ApplicationDbContext context) : IServiceBookingService
{
    public async Task<ServiceBookingDto> CreateBookingAsync(ServiceBookingCreateDto dto, string userId, CancellationToken ct = default)
    {
        var booking = new ServiceBooking
        {
            EquipmentId = dto.EquipmentId,
            EquipmentInspectionId = dto.EquipmentInspectionId,
            ServiceProvider = dto.ServiceProvider,
            Reason = dto.Reason,
            Status = ServiceBookingStatus.Sent,
            SentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpectedReturnDate = dto.ExpectedReturnDate,
            Notes = dto.Notes,
            CreatedById = userId
        };

        context.ServiceBookings.Add(booking);

        var equipment = await context.Equipment.FindAsync([dto.EquipmentId], ct);
        if (equipment is not null)
        {
            equipment.LastServiceDate = booking.SentDate;
            equipment.ModifiedById = userId;
            equipment.ModifiedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(ct);

        return (await GetBookingsForEquipmentAsync(dto.EquipmentId, ct)).First(b => b.Id == booking.Id);
    }

    public async Task UpdateBookingStatusAsync(ServiceBookingUpdateStatusDto dto, string userId, CancellationToken ct = default)
    {
        var booking = await context.ServiceBookings.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Service booking {dto.Id} not found");

        booking.Status = dto.Status;
        booking.Notes = dto.Notes ?? booking.Notes;
        booking.ModifiedById = userId;
        booking.ModifiedAt = DateTime.UtcNow;

        if (dto.Status == ServiceBookingStatus.Returned)
        {
            booking.ActualReturnDate = dto.ActualReturnDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task<List<ServiceBookingDto>> GetBookingsForEquipmentAsync(int equipmentId, CancellationToken ct = default)
    {
        return await context.ServiceBookings
            .Where(sb => sb.EquipmentId == equipmentId)
            .OrderByDescending(sb => sb.SentDate)
            .Select(sb => new ServiceBookingDto(
                sb.Id, sb.EquipmentId, sb.Equipment.Identifier,
                sb.ServiceProvider, sb.Reason, sb.Status,
                sb.SentDate, sb.ExpectedReturnDate, sb.ActualReturnDate,
                sb.Notes, sb.CreatedById, sb.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<List<ServiceBookingOverviewDto>> GetActiveBookingsAsync(CancellationToken ct = default)
    {
        return await context.ServiceBookings
            .Where(sb => sb.Status == ServiceBookingStatus.Sent || sb.Status == ServiceBookingStatus.InService)
            .OrderByDescending(sb => sb.SentDate)
            .Select(sb => new ServiceBookingOverviewDto(
                sb.Id, sb.EquipmentId,
                sb.Equipment.Identifier,
                sb.Equipment.EquipmentType.Name,
                sb.Equipment.EquipmentSubType != null ? sb.Equipment.EquipmentSubType.Name : null,
                sb.Equipment.Section.Plant.Name,
                sb.Equipment.Section.Name,
                sb.ServiceProvider, sb.Reason, sb.Status,
                sb.SentDate, sb.ExpectedReturnDate))
            .ToListAsync(ct);
    }

    public async Task<List<ServiceBookingFullDto>> GetAllBookingsAsync(ServiceBookingStatus? status = null, CancellationToken ct = default)
    {
        var query = context.ServiceBookings.AsQueryable();
        if (status.HasValue)
            query = query.Where(sb => sb.Status == status.Value);

        return await query
            .OrderByDescending(sb => sb.SentDate)
            .Select(sb => new ServiceBookingFullDto(
                sb.Id, sb.EquipmentId,
                sb.Equipment.Identifier,
                sb.Equipment.EquipmentType.Name,
                sb.Equipment.EquipmentSubType != null ? sb.Equipment.EquipmentSubType.Name : null,
                sb.Equipment.Section.Plant.Name,
                sb.Equipment.Section.Name,
                sb.ServiceProvider, sb.Reason, sb.Status,
                sb.SentDate, sb.ExpectedReturnDate, sb.ActualReturnDate,
                sb.Notes, sb.CreatedAt))
            .ToListAsync(ct);
    }
}
