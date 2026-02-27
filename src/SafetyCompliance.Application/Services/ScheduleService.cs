using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class ScheduleService(ApplicationDbContext context) : IScheduleService
{
    public async Task<List<InspectionScheduleDto>> GetSchedulesAsync(int? plantId = null, CancellationToken ct = default)
    {
        var query = context.InspectionSchedules.AsQueryable();
        if (plantId.HasValue)
            query = query.Where(s => s.PlantId == plantId.Value);

        return await query
            .OrderBy(s => s.NextDueDate)
            .Select(s => new InspectionScheduleDto(
                s.Id, s.PlantId, s.Plant.Name, s.Name, s.Description,
                s.Frequency, s.FrequencyInterval, s.StartDate, s.EndDate,
                s.NextDueDate, s.LastCompletedDate, s.IsActive, s.AutoGenerate,
                s.InspectionRounds.Count(r => r.Status == InspectionStatus.Completed || r.Status == InspectionStatus.Reviewed),
                s.InspectionRounds.Count))
            .ToListAsync(ct);
    }

    public async Task<InspectionScheduleDto?> GetScheduleByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.InspectionSchedules
            .Where(s => s.Id == id)
            .Select(s => new InspectionScheduleDto(
                s.Id, s.PlantId, s.Plant.Name, s.Name, s.Description,
                s.Frequency, s.FrequencyInterval, s.StartDate, s.EndDate,
                s.NextDueDate, s.LastCompletedDate, s.IsActive, s.AutoGenerate,
                s.InspectionRounds.Count(r => r.Status == InspectionStatus.Completed || r.Status == InspectionStatus.Reviewed),
                s.InspectionRounds.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<InspectionScheduleDto> CreateScheduleAsync(InspectionScheduleCreateDto dto, string userId, CancellationToken ct = default)
    {
        var schedule = new InspectionSchedule
        {
            PlantId = dto.PlantId,
            Name = dto.Name,
            Description = dto.Description,
            Frequency = dto.Frequency,
            FrequencyInterval = dto.FrequencyInterval,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            NextDueDate = dto.StartDate,
            AutoGenerate = dto.AutoGenerate,
            CreatedById = userId
        };

        context.InspectionSchedules.Add(schedule);
        await context.SaveChangesAsync(ct);

        return (await GetScheduleByIdAsync(schedule.Id, ct))!;
    }

    public async Task UpdateScheduleAsync(InspectionScheduleUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var schedule = await context.InspectionSchedules.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Schedule {dto.Id} not found");

        schedule.Name = dto.Name;
        schedule.Description = dto.Description;
        schedule.Frequency = dto.Frequency;
        schedule.FrequencyInterval = dto.FrequencyInterval;
        schedule.EndDate = dto.EndDate;
        schedule.IsActive = dto.IsActive;
        schedule.AutoGenerate = dto.AutoGenerate;
        schedule.ModifiedById = userId;
        schedule.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteScheduleAsync(int id, CancellationToken ct = default)
    {
        var schedule = await context.InspectionSchedules.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Schedule {id} not found");

        schedule.IsActive = false;
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<CalendarEventDto>> GetCalendarEventsAsync(DateOnly from, DateOnly to, int? plantId = null, CancellationToken ct = default)
    {
        var events = new List<CalendarEventDto>();

        // Get completed/in-progress inspection rounds
        var roundsQuery = context.InspectionRounds
            .Where(r => r.InspectionDate >= from && r.InspectionDate <= to);
        if (plantId.HasValue)
            roundsQuery = roundsQuery.Where(r => r.PlantId == plantId.Value);

        var rounds = await roundsQuery
            .Select(r => new CalendarEventDto(
                r.Id,
                r.Plant.Name + " - " + r.InspectionMonth,
                r.InspectionDate,
                "inspection",
                r.Status,
                r.PlantId,
                r.Plant.Name,
                r.InspectionScheduleId,
                r.Status == InspectionStatus.Draft || r.Status == InspectionStatus.InProgress))
            .ToListAsync(ct);

        events.AddRange(rounds);

        // Get scheduled (future) from active schedules
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedules = await context.InspectionSchedules
            .Where(s => s.IsActive && s.NextDueDate >= from && s.NextDueDate <= to)
            .Where(s => !plantId.HasValue || s.PlantId == plantId.Value)
            .Select(s => new { s.Id, s.PlantId, PlantName = s.Plant.Name, s.Name, s.NextDueDate })
            .ToListAsync(ct);

        foreach (var sched in schedules)
        {
            // Only add if no round exists for this date
            if (!events.Any(e => e.ScheduleId == sched.Id && e.Date == sched.NextDueDate))
            {
                events.Add(new CalendarEventDto(
                    sched.Id, sched.PlantName + " - " + sched.Name, sched.NextDueDate,
                    "scheduled", null, sched.PlantId, sched.PlantName, sched.Id,
                    sched.NextDueDate < today));
            }
        }

        return events.OrderBy(e => e.Date).ToList();
    }

    public async Task<List<TimelineItemDto>> GetTimelineAsync(int count = 20, int? plantId = null, CancellationToken ct = default)
    {
        var items = new List<TimelineItemDto>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Upcoming and recent rounds
        var roundsQuery = context.InspectionRounds
            .Where(r => r.InspectionDate >= today.AddDays(-30));
        if (plantId.HasValue)
            roundsQuery = roundsQuery.Where(r => r.PlantId == plantId.Value);

        var rounds = await roundsQuery
            .OrderBy(r => r.InspectionDate)
            .Take(count)
            .Select(r => new TimelineItemDto(
                r.Id, r.Plant.Name + " Inspection", r.Notes, r.InspectionDate,
                "inspection", r.Status, null, null,
                r.PlantId, r.Plant.Name, null,
                r.EquipmentInspections.Count(ei => ei.IsComplete),
                r.EquipmentInspections.Count,
                r.Status != InspectionStatus.Completed && r.Status != InspectionStatus.Reviewed && r.InspectionDate < today))
            .ToListAsync(ct);

        items.AddRange(rounds);

        // Upcoming scheduled
        var schedules = await context.InspectionSchedules
            .Where(s => s.IsActive && s.NextDueDate >= today)
            .Where(s => !plantId.HasValue || s.PlantId == plantId.Value)
            .OrderBy(s => s.NextDueDate)
            .Take(count)
            .Select(s => new TimelineItemDto(
                s.Id, s.Name, s.Description, s.NextDueDate,
                "scheduled", null, null, null,
                s.PlantId, s.Plant.Name, null, 0, 0, false))
            .ToListAsync(ct);

        items.AddRange(schedules);

        // Open issues
        var issues = await context.Issues
            .Where(i => i.Status == IssueStatus.Open || i.Status == IssueStatus.InProgress)
            .Where(i => !plantId.HasValue || (i.Equipment != null && i.Equipment.Section.Plant.Id == plantId.Value))
            .OrderBy(i => i.Priority == IssuePriority.Critical ? 0 : i.Priority == IssuePriority.High ? 1 : 2)
            .ThenBy(i => i.DueDate)
            .Take(count)
            .Select(i => new TimelineItemDto(
                i.Id, i.Title, i.Description,
                i.DueDate ?? DateOnly.FromDateTime(i.CreatedAt),
                "issue", null, i.Priority, i.Status,
                0, i.Equipment != null ? i.Equipment.Section.Plant.Name : "N/A",
                i.Equipment != null ? i.Equipment.Identifier : null,
                0, 0,
                i.DueDate.HasValue && i.DueDate.Value < today))
            .ToListAsync(ct);

        items.AddRange(issues);

        return items.OrderBy(i => i.Date).Take(count).ToList();
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var scheduledThisMonth = await context.InspectionRounds
            .CountAsync(r => r.InspectionDate >= monthStart && r.InspectionDate <= monthEnd, ct);

        var completedThisMonth = await context.InspectionRounds
            .CountAsync(r => r.InspectionDate >= monthStart && r.InspectionDate <= monthEnd &&
                (r.Status == InspectionStatus.Completed || r.Status == InspectionStatus.Reviewed), ct);

        var overdueInspections = await context.InspectionSchedules
            .CountAsync(s => s.IsActive && s.NextDueDate < today, ct);

        var openIssues = await context.Issues
            .CountAsync(i => i.Status == IssueStatus.Open || i.Status == IssueStatus.InProgress, ct);

        var criticalIssues = await context.Issues
            .CountAsync(i => (i.Status == IssueStatus.Open || i.Status == IssueStatus.InProgress) && i.Priority == IssuePriority.Critical, ct);

        var resolvedThisMonth = await context.Issues
            .CountAsync(i => i.ResolvedAt.HasValue &&
                i.ResolvedAt.Value.Year == today.Year && i.ResolvedAt.Value.Month == today.Month, ct);

        return new DashboardStatsDto(scheduledThisMonth, completedThisMonth, overdueInspections,
            openIssues, criticalIssues, resolvedThisMonth);
    }
}
