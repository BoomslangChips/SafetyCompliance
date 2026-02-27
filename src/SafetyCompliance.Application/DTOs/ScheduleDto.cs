using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.DTOs;

public record InspectionScheduleDto(
    int Id, int PlantId, string PlantName, string Name, string? Description,
    FrequencyType Frequency, int FrequencyInterval, DateOnly StartDate, DateOnly? EndDate,
    DateOnly NextDueDate, DateOnly? LastCompletedDate, bool IsActive, bool AutoGenerate,
    int CompletedRounds, int TotalRounds);

public record InspectionScheduleCreateDto(
    int PlantId, string Name, string? Description, FrequencyType Frequency,
    int FrequencyInterval, DateOnly StartDate, DateOnly? EndDate, bool AutoGenerate);

public record InspectionScheduleUpdateDto(
    int Id, string Name, string? Description, FrequencyType Frequency,
    int FrequencyInterval, DateOnly? EndDate, bool IsActive, bool AutoGenerate);
