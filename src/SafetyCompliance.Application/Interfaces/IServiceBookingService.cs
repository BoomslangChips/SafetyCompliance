using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IServiceBookingService
{
    Task<ServiceBookingDto> CreateBookingAsync(ServiceBookingCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateBookingStatusAsync(ServiceBookingUpdateStatusDto dto, string userId, CancellationToken ct = default);
    Task<List<ServiceBookingDto>> GetBookingsForEquipmentAsync(int equipmentId, CancellationToken ct = default);
    Task<List<ServiceBookingOverviewDto>> GetActiveBookingsAsync(CancellationToken ct = default);
}
