using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.DTOs;

public record CalendarEventDto(
    int Id, string Title, DateOnly Date, string EventType,
    InspectionStatus? InspectionStatus, int PlantId, string PlantName,
    int? ScheduleId, bool IsOverdue);

public record TimelineItemDto(
    int Id, string Title, string? Description, DateOnly Date, string EventType,
    InspectionStatus? InspectionStatus, IssuePriority? Priority, IssueStatus? IssueStatus,
    int PlantId, string PlantName, string? EquipmentIdentifier,
    int CompletedItems, int TotalItems, bool IsOverdue);

public record DashboardStatsDto(
    int ScheduledThisMonth, int CompletedThisMonth, int OverdueInspections,
    int OpenIssues, int CriticalIssues, int ResolvedThisMonth);
