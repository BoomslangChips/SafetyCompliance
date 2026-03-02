using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.Interfaces;

public interface IServiceBookingService
{
    Task<ServiceBookingDto> CreateBookingAsync(ServiceBookingCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateBookingStatusAsync(ServiceBookingUpdateStatusDto dto, string userId, CancellationToken ct = default);
    Task<List<ServiceBookingDto>> GetBookingsForEquipmentAsync(int equipmentId, CancellationToken ct = default);
    Task<List<ServiceBookingOverviewDto>> GetActiveBookingsAsync(CancellationToken ct = default);
    Task<List<ServiceBookingFullDto>> GetAllBookingsAsync(ServiceBookingStatus? status = null, CancellationToken ct = default);
}
