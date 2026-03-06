using SafetyCompliance.Application.DTOs;

namespace SafetyCompliance.Application.Interfaces;

public interface IExcelExportService
{
    /// <summary>
    /// Exports the all-plant current-month summary to a single-sheet workbook.
    /// </summary>
    /// <param name="summaries">The list of plant summary snapshots.</param>
    /// <param name="period">Human-readable period string shown in the sheet title, e.g. "March 2026".</param>
    /// <returns>Raw bytes of the .xlsx file.</returns>
    byte[] ExportSummary(List<PlantReportSummaryDto> summaries, string period);

    /// <summary>
    /// Exports a full monthly maintenance report to a multi-sheet workbook.
    /// When <paramref name="photoFiles"/> is provided, an Inspection Photos sheet
    /// with embedded images is included.
    /// </summary>
    byte[] ExportMonthlyReport(MonthlyReportDto report, Dictionary<string, byte[]>? photoFiles = null);

    /// <summary>
    /// Exports equipment inventory data to a single-sheet workbook.
    /// </summary>
    /// <param name="equipment">The list of inventory equipment items.</param>
    /// <returns>Raw bytes of the .xlsx file.</returns>
    byte[] ExportInventory(List<InventoryEquipmentDto> equipment);
}
