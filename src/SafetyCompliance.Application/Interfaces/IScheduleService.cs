using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IScheduleService
{
    Task<List<InspectionScheduleDto>> GetSchedulesAsync(int? plantId = null, CancellationToken ct = default);
    Task<InspectionScheduleDto?> GetScheduleByIdAsync(int id, CancellationToken ct = default);
    Task<InspectionScheduleDto> CreateScheduleAsync(InspectionScheduleCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateScheduleAsync(InspectionScheduleUpdateDto dto, string userId, CancellationToken ct = default);
    Task DeleteScheduleAsync(int id, CancellationToken ct = default);
    Task<List<CalendarEventDto>> GetCalendarEventsAsync(DateOnly from, DateOnly to, int? plantId = null, CancellationToken ct = default);
    Task<List<TimelineItemDto>> GetTimelineAsync(int count = 20, int? plantId = null, CancellationToken ct = default);
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
}
