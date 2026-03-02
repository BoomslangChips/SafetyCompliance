using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IReportService
{
    /// <summary>Returns a current-month snapshot card for each plant.</summary>
    Task<List<PlantReportSummaryDto>> GetPlantSummariesAsync(
        int? companyId = null,
        CancellationToken ct = default);

    /// <summary>Returns the full monthly maintenance report for one plant.</summary>
    Task<MonthlyReportDto?> GetMonthlyReportAsync(
        int plantId,
        int year,
        int month,
        CancellationToken ct = default);
}
