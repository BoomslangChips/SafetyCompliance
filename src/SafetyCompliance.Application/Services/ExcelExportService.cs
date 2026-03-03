using ClosedXML.Excel;
using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;

namespace SafetyCompliance.Application.Services;

/// <summary>
/// Generates .xlsx workbooks from report DTOs using ClosedXML.
/// All public methods are synchronous (CPU-only, no I/O) and return raw byte arrays
/// that the caller can base-64 encode and push to the browser via JS interop.
/// </summary>
public class ExcelExportService : IExcelExportService
{
    // ── shared colours ────────────────────────────────────────────────────────
    private static readonly XLColor HeaderFill      = XLColor.FromHtml("#E8E8E8");
    private static readonly XLColor AccentFill      = XLColor.FromHtml("#1E3A5F");  // dark-navy header band
    private static readonly XLColor GreenFill       = XLColor.FromHtml("#C6EFCE");
    private static readonly XLColor AmberFill       = XLColor.FromHtml("#FFEB9C");
    private static readonly XLColor RedFill         = XLColor.FromHtml("#FFC7CE");
    private static readonly XLColor GreenFont       = XLColor.FromHtml("#276221");
    private static readonly XLColor AmberFont       = XLColor.FromHtml("#9C6500");
    private static readonly XLColor RedFont         = XLColor.FromHtml("#9C0006");

    // ═══════════════════════════════════════════════════════════════════════════
    //  SUMMARY EXPORT  — one sheet, one row per plant
    // ═══════════════════════════════════════════════════════════════════════════

    public byte[] ExportSummary(List<PlantReportSummaryDto> summaries, string period)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Plant Summaries");

        // ── Title band ────────────────────────────────────────────────────────
        ws.Cell(1, 1).Value = $"MIS — Plant Summary Report — {period}";
        ws.Range(1, 1, 1, 12).Merge();
        StyleTitleRow(ws.Row(1), AccentFill);
        ws.Row(1).Height = 22;

        // ── Column headers (row 2) ────────────────────────────────────────────
        var headers = new[]
        {
            "Company", "Plant", "Compliance %",
            "Inspections Done", "Total Inspections",
            "Equipment Inspected", "Total Equipment",
            "Open Incidents", "Critical Incidents",
            "Active Service", "Notes",
            "Last Inspection Date"
        };
        for (int c = 0; c < headers.Length; c++)
            ws.Cell(2, c + 1).Value = headers[c];

        StyleHeaderRow(ws.Row(2));
        ws.SheetView.FreezeRows(2);

        // ── Data rows (from row 3) ────────────────────────────────────────────
        int row = 3;
        foreach (var s in summaries)
        {
            ws.Cell(row, 1).Value  = s.CompanyName;
            ws.Cell(row, 2).Value  = s.PlantName;
            ws.Cell(row, 3).Value  = s.CompliancePct;          // Compliance %
            ws.Cell(row, 4).Value  = s.InspectionsCompleted;
            ws.Cell(row, 5).Value  = s.InspectionsTotal;
            ws.Cell(row, 6).Value  = s.EquipmentInspected;
            ws.Cell(row, 7).Value  = s.TotalEquipment;
            ws.Cell(row, 8).Value  = s.OpenIncidents;
            ws.Cell(row, 9).Value  = s.CriticalIncidents;
            ws.Cell(row, 10).Value = s.ActiveServiceBookings;
            ws.Cell(row, 11).Value = s.NotesCount;
            ws.Cell(row, 12).Value = s.LastInspectionDate.HasValue
                ? s.LastInspectionDate.Value.ToString("dd MMM yyyy")
                : "—";

            // Colour-code Compliance %
            ApplyComplianceColour(ws.Cell(row, 3), s.CompliancePct);

            // Highlight critical rows
            if (s.CriticalIncidents > 0)
                ws.Cell(row, 9).Style.Fill.BackgroundColor = RedFill;

            row++;
        }

        // ── Totals / summary row ──────────────────────────────────────────────
        if (summaries.Count > 0)
        {
            ws.Cell(row, 1).Value = "TOTAL / AVERAGE";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = summaries.Count > 0
                ? (int)Math.Round(summaries.Average(s => s.CompliancePct))
                : 0;
            ws.Cell(row, 3).Style.Font.Bold = true;
            ApplyComplianceColour(ws.Cell(row, 3),
                summaries.Count > 0 ? (int)Math.Round(summaries.Average(s => s.CompliancePct)) : 0);
            ws.Cell(row, 4).Value  = summaries.Sum(s => s.InspectionsCompleted);
            ws.Cell(row, 5).Value  = summaries.Sum(s => s.InspectionsTotal);
            ws.Cell(row, 6).Value  = summaries.Sum(s => s.EquipmentInspected);
            ws.Cell(row, 7).Value  = summaries.Sum(s => s.TotalEquipment);
            ws.Cell(row, 8).Value  = summaries.Sum(s => s.OpenIncidents);
            ws.Cell(row, 9).Value  = summaries.Sum(s => s.CriticalIncidents);
            ws.Cell(row, 10).Value = summaries.Sum(s => s.ActiveServiceBookings);
            ws.Cell(row, 11).Value = summaries.Sum(s => s.NotesCount);
            ws.Range(row, 1, row, 12).Style.Fill.BackgroundColor = HeaderFill;
        }

        // ── Auto-fit and borders ──────────────────────────────────────────────
        ws.Columns().AdjustToContents();
        ws.Column(3).Width = Math.Max(ws.Column(3).Width, 14); // Compliance %

        var dataRange = ws.Range(2, 1, row, 12);
        dataRange.Style.Border.OutsideBorder  = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder   = XLBorderStyleValues.Hair;
        dataRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#AAAAAA");
        dataRange.Style.Border.InsideBorderColor  = XLColor.FromHtml("#DDDDDD");

        return ToBytes(wb);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  MONTHLY REPORT EXPORT  — 6 sheets
    // ═══════════════════════════════════════════════════════════════════════════

    public byte[] ExportMonthlyReport(MonthlyReportDto report)
    {
        using var wb = new XLWorkbook();

        var period = new DateTime(report.Year, report.Month, 1).ToString("MMMM yyyy");
        var title  = $"{report.PlantName} — {period}";

        AddSummarySheet(wb, report, title);
        AddRoundsSheet(wb, report, title);
        AddEquipmentSheet(wb, report, title);
        AddIssuesSheet(wb, report, title);
        AddServiceSheet(wb, report, title);
        AddNotesSheet(wb, report, title);
        AddContactsSheet(wb, report, title);

        return ToBytes(wb);
    }

    // ── Sheet 1: Summary ─────────────────────────────────────────────────────

    private static void AddSummarySheet(XLWorkbook wb, MonthlyReportDto r, string title)
    {
        var ws = wb.Worksheets.Add("Summary");

        ws.Cell(1, 1).Value = $"MIS — Monthly Maintenance Report — {title}";
        ws.Range(1, 1, 1, 3).Merge();
        StyleTitleRow(ws.Row(1), AccentFill);
        ws.Row(1).Height = 22;

        var period = new DateTime(r.Year, r.Month, 1).ToString("MMMM yyyy");

        var kpis = new (string Label, string Value)[]
        {
            ("Plant",               r.PlantName),
            ("Company",             r.CompanyName),
            ("Report Period",       period),
            ("Contact",             r.ContactName ?? "—"),
            ("Contact Phone",       r.ContactPhone ?? "—"),
            ("Total Equipment",     r.TotalEquipmentInPlant.ToString()),
            ("Overall Compliance",  $"{r.CompliancePct}%"),
            ("Rounds Completed",    $"{r.Rounds.Count(rd => rd.Status is "Completed" or "Reviewed" or "CompletedWithIssues")} / {r.Rounds.Count}"),
            ("Equipment Inspected", r.Equipment.Count(e => e.IsComplete).ToString()),
            ("Open Incidents",      r.Issues.Count(i => i.Status is "Open" or "InProgress").ToString()),
            ("Active Service",      r.ServiceBookings.Count(b => b.Status is "Sent" or "InService").ToString()),
            ("Notes",               r.Notes.Count.ToString()),
            ("Generated At",        r.GeneratedAt.ToString("dd MMM yyyy HH:mm")),
        };

        int row = 3;
        ws.Cell(row, 1).Value = "Key Performance Indicator";
        ws.Cell(row, 2).Value = "Value";
        StyleHeaderRow(ws.Row(row));
        row++;

        foreach (var (label, value) in kpis)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 2).Value = value;

            // Colour compliance row
            if (label == "Overall Compliance")
            {
                int pct = r.CompliancePct;
                ApplyComplianceColour(ws.Cell(row, 2), pct);
            }
            row++;
        }

        ws.Column(1).Width = 28;
        ws.Column(2).Width = 36;
        ws.Column(1).Style.Font.Bold = true;
        ApplyBorder(ws.Range(3, 1, row - 1, 2));
    }

    // ── Sheet 2: Inspection Rounds ───────────────────────────────────────────

    private static void AddRoundsSheet(XLWorkbook wb, MonthlyReportDto r, string title)
    {
        var ws = wb.Worksheets.Add("Inspection Rounds");
        AddSheetTitle(ws, $"Inspection Rounds — {title}", 9);

        var headers = new[]
        {
            "Round ID", "Date", "Status", "Inspector",
            "Equipment Done", "Total Equipment",
            "Failed Checks", "Total Checks", "Completed At"
        };
        WriteHeaderRow(ws, 2, headers);

        int row = 3;
        foreach (var rd in r.Rounds)
        {
            ws.Cell(row, 1).Value = rd.Id;
            ws.Cell(row, 2).Value = rd.Date.ToString("dd MMM yyyy");
            ws.Cell(row, 3).Value = FormatStatus(rd.Status);
            ws.Cell(row, 4).Value = rd.InspectorName;
            ws.Cell(row, 5).Value = rd.CompletedEquipment;
            ws.Cell(row, 6).Value = rd.TotalEquipment;
            ws.Cell(row, 7).Value = rd.FailedChecks;
            ws.Cell(row, 8).Value = rd.TotalChecks;
            ws.Cell(row, 9).Value = rd.CompletedAt.HasValue
                ? rd.CompletedAt.Value.ToString("dd MMM yyyy HH:mm")
                : "—";

            // Colour status cell
            var statusCell = ws.Cell(row, 3);
            statusCell.Style.Fill.BackgroundColor = rd.Status switch
            {
                "Completed" or "Reviewed"     => GreenFill,
                "CompletedWithIssues"          => AmberFill,
                "InProgress"                   => XLColor.FromHtml("#DEEBF7"),
                _                              => XLColor.NoColor
            };
            if (rd.FailedChecks > 0)
                ws.Cell(row, 7).Style.Fill.BackgroundColor = AmberFill;

            row++;
        }

        if (r.Rounds.Count == 0)
        { ws.Cell(3, 1).Value = "No inspection rounds recorded for this period."; }

        FinaliseSheet(ws, row - 1, headers.Length);
    }

    // ── Sheet 3: Equipment Checklist ─────────────────────────────────────────

    private static void AddEquipmentSheet(XLWorkbook wb, MonthlyReportDto r, string title)
    {
        var ws = wb.Worksheets.Add("Equipment Checklist");
        AddSheetTitle(ws, $"Equipment Checklist — {title}", 9);

        var headers = new[]
        {
            "Round ID", "Identifier", "Type", "Section",
            "Complete?", "Pass Checks", "Fail Checks",
            "Failed Items", "Comments"
        };
        WriteHeaderRow(ws, 2, headers);

        int row = 3;
        foreach (var eq in r.Equipment)
        {
            ws.Cell(row, 1).Value = eq.RoundId;
            ws.Cell(row, 2).Value = eq.Identifier;
            ws.Cell(row, 3).Value = eq.TypeName;
            ws.Cell(row, 4).Value = eq.SectionName;
            ws.Cell(row, 5).Value = eq.IsComplete ? "Yes" : "No";
            ws.Cell(row, 6).Value = eq.PassChecks;
            ws.Cell(row, 7).Value = eq.FailChecks;
            ws.Cell(row, 8).Value = eq.FailedItems.Count > 0
                ? string.Join(", ", eq.FailedItems)
                : "—";
            ws.Cell(row, 9).Value = eq.Comments ?? "—";

            // Colour complete / fail cells
            if (eq.IsComplete)
                ws.Cell(row, 5).Style.Fill.BackgroundColor = GreenFill;
            else
                ws.Cell(row, 5).Style.Fill.BackgroundColor = AmberFill;

            if (eq.FailChecks > 0)
                ws.Cell(row, 7).Style.Fill.BackgroundColor = RedFill;

            row++;
        }

        if (r.Equipment.Count == 0)
        { ws.Cell(3, 1).Value = "No equipment inspections recorded for this period."; }

        FinaliseSheet(ws, row - 1, headers.Length);
        // Wrap text in Failed Items and Comments columns
        ws.Column(8).Style.Alignment.WrapText = true;
        ws.Column(8).Width = 40;
        ws.Column(9).Style.Alignment.WrapText = true;
        ws.Column(9).Width = 35;
    }

    // ── Sheet 4: Incidents & Issues ──────────────────────────────────────────

    private static void AddIssuesSheet(XLWorkbook wb, MonthlyReportDto r, string title)
    {
        var ws = wb.Worksheets.Add("Incidents & Issues");
        AddSheetTitle(ws, $"Incidents & Issues — {title}", 9);

        var headers = new[]
        {
            "ID", "Title", "Priority", "Status",
            "Assigned To", "Due Date", "Equipment",
            "Created At", "Resolved At"
        };
        WriteHeaderRow(ws, 2, headers);

        int row = 3;
        foreach (var i in r.Issues)
        {
            ws.Cell(row, 1).Value = i.Id;
            ws.Cell(row, 2).Value = i.Title;
            ws.Cell(row, 3).Value = i.Priority;
            ws.Cell(row, 4).Value = FormatStatus(i.Status);
            ws.Cell(row, 5).Value = i.AssignedTo ?? "—";
            ws.Cell(row, 6).Value = i.DueDate.HasValue ? i.DueDate.Value.ToString("dd MMM yyyy") : "—";
            ws.Cell(row, 7).Value = i.EquipmentIdentifier ?? "—";
            ws.Cell(row, 8).Value = i.CreatedAt.ToString("dd MMM yyyy");
            ws.Cell(row, 9).Value = i.ResolvedAt.HasValue ? i.ResolvedAt.Value.ToString("dd MMM yyyy") : "—";

            // Priority colour
            ws.Cell(row, 3).Style.Fill.BackgroundColor = i.Priority switch
            {
                "Critical" => RedFill,
                "High"     => XLColor.FromHtml("#FCE4D6"),
                "Medium"   => AmberFill,
                _          => GreenFill
            };
            ws.Cell(row, 3).Style.Font.FontColor = i.Priority switch
            {
                "Critical" => RedFont,
                _          => XLColor.Black
            };

            // Status colour
            ws.Cell(row, 4).Style.Fill.BackgroundColor = i.Status switch
            {
                "Open"       => XLColor.FromHtml("#FCE4D6"),
                "InProgress" => AmberFill,
                "Resolved"   => GreenFill,
                _            => XLColor.NoColor
            };

            // Overdue
            if (i.DueDate.HasValue
                && i.DueDate.Value < DateOnly.FromDateTime(DateTime.Today)
                && i.Status is "Open" or "InProgress")
            {
                ws.Cell(row, 6).Style.Fill.BackgroundColor = RedFill;
                ws.Cell(row, 6).Style.Font.FontColor = RedFont;
            }

            row++;
        }

        if (r.Issues.Count == 0)
        { ws.Cell(3, 1).Value = "No incidents or issues recorded for this plant."; }

        FinaliseSheet(ws, row - 1, headers.Length);
        ws.Column(2).Width = 40; // Title — let it breathe
    }

    // ── Sheet 5: Service Bookings ────────────────────────────────────────────

    private static void AddServiceSheet(XLWorkbook wb, MonthlyReportDto r, string title)
    {
        var ws = wb.Worksheets.Add("Service Bookings");
        AddSheetTitle(ws, $"Service Bookings — {title}", 10);

        var headers = new[]
        {
            "ID", "Equipment", "Type", "Section",
            "Provider", "Reason", "Status",
            "Sent Date", "Expected Return", "Actual Return"
        };
        WriteHeaderRow(ws, 2, headers);

        int row = 3;
        foreach (var b in r.ServiceBookings)
        {
            ws.Cell(row, 1).Value  = b.Id;
            ws.Cell(row, 2).Value  = b.EquipmentIdentifier;
            ws.Cell(row, 3).Value  = b.TypeName;
            ws.Cell(row, 4).Value  = b.SectionName;
            ws.Cell(row, 5).Value  = b.Provider;
            ws.Cell(row, 6).Value  = b.Reason;
            ws.Cell(row, 7).Value  = FormatStatus(b.Status);
            ws.Cell(row, 8).Value  = b.SentDate.ToString("dd MMM yyyy");
            ws.Cell(row, 9).Value  = b.ExpectedReturn.HasValue ? b.ExpectedReturn.Value.ToString("dd MMM yyyy") : "—";
            ws.Cell(row, 10).Value = b.ActualReturn.HasValue   ? b.ActualReturn.Value.ToString("dd MMM yyyy")   : "—";

            // Status colour
            ws.Cell(row, 7).Style.Fill.BackgroundColor = b.Status switch
            {
                "Sent" or "InService" => AmberFill,
                "Returned"            => GreenFill,
                "Cancelled"           => XLColor.FromHtml("#F2F2F2"),
                _                     => XLColor.NoColor
            };

            // Overdue expected return
            if (b.ExpectedReturn.HasValue
                && b.ExpectedReturn.Value < DateOnly.FromDateTime(DateTime.Today)
                && b.Status is "Sent" or "InService")
            {
                ws.Cell(row, 9).Style.Fill.BackgroundColor = RedFill;
                ws.Cell(row, 9).Style.Font.FontColor = RedFont;
            }

            row++;
        }

        if (r.ServiceBookings.Count == 0)
        { ws.Cell(3, 1).Value = "No service bookings recorded for this period."; }

        FinaliseSheet(ws, row - 1, headers.Length);
        ws.Column(6).Width = 35; // Reason
    }

    // ── Sheet 6: Notes ───────────────────────────────────────────────────────

    private static void AddNotesSheet(XLWorkbook wb, MonthlyReportDto r, string title)
    {
        var ws = wb.Worksheets.Add("Notes");
        AddSheetTitle(ws, $"Notes — {title}", 8);

        var headers = new[]
        {
            "ID", "Title", "Category", "Priority",
            "Pinned", "Equipment", "Content", "Created At"
        };
        WriteHeaderRow(ws, 2, headers);

        int row = 3;
        foreach (var n in r.Notes)
        {
            ws.Cell(row, 1).Value = n.Id;
            ws.Cell(row, 2).Value = n.Title;
            ws.Cell(row, 3).Value = n.Category;
            ws.Cell(row, 4).Value = n.Priority;
            ws.Cell(row, 5).Value = n.IsPinned ? "Yes" : "No";
            ws.Cell(row, 6).Value = n.EquipmentIdentifier ?? "—";
            ws.Cell(row, 7).Value = n.Content;
            ws.Cell(row, 8).Value = n.CreatedAt.ToString("dd MMM yyyy");

            // Priority colour
            ws.Cell(row, 4).Style.Fill.BackgroundColor = n.Priority switch
            {
                "Urgent"    => RedFill,
                "Important" => AmberFill,
                _           => XLColor.NoColor
            };

            if (n.IsPinned)
                ws.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#DEEBF7");

            row++;
        }

        if (r.Notes.Count == 0)
        { ws.Cell(3, 1).Value = "No notes recorded for this plant."; }

        FinaliseSheet(ws, row - 1, headers.Length);
        ws.Column(7).Style.Alignment.WrapText = true;
        ws.Column(7).Width = 60; // Content column
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private static void AddSheetTitle(IXLWorksheet ws, string text, int colCount)
    {
        ws.Cell(1, 1).Value = text;
        ws.Range(1, 1, 1, colCount).Merge();
        StyleTitleRow(ws.Row(1), AccentFill);
        ws.Row(1).Height = 20;
    }

    private static void WriteHeaderRow(IXLWorksheet ws, int row, string[] headers)
    {
        for (int c = 0; c < headers.Length; c++)
            ws.Cell(row, c + 1).Value = headers[c];
        StyleHeaderRow(ws.Row(row));
        ws.SheetView.FreezeRows(row);
    }

    private static void FinaliseSheet(IXLWorksheet ws, int lastDataRow, int colCount)
    {
        ws.Columns(1, colCount).AdjustToContents();
        // Cap very wide columns to keep the sheet usable
        for (int c = 1; c <= colCount; c++)
            if (ws.Column(c).Width > 55) ws.Column(c).Width = 55;

        if (lastDataRow >= 2)
            ApplyBorder(ws.Range(2, 1, lastDataRow, colCount));
    }

    private static void StyleTitleRow(IXLRow row, XLColor fill)
    {
        row.Style.Font.Bold      = true;
        row.Style.Font.FontSize  = 12;
        row.Style.Font.FontColor = XLColor.White;
        row.Style.Fill.BackgroundColor = fill;
        row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        row.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
    }

    private static void StyleHeaderRow(IXLRow row)
    {
        row.Style.Font.Bold = true;
        row.Style.Fill.BackgroundColor = HeaderFill;
        row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        row.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        row.Height = 16;
    }

    private static void ApplyComplianceColour(IXLCell cell, int pct)
    {
        if (pct >= 90)
        {
            cell.Style.Fill.BackgroundColor = GreenFill;
            cell.Style.Font.FontColor           = GreenFont;
        }
        else if (pct >= 70)
        {
            cell.Style.Fill.BackgroundColor = AmberFill;
            cell.Style.Font.FontColor           = AmberFont;
        }
        else
        {
            cell.Style.Fill.BackgroundColor = RedFill;
            cell.Style.Font.FontColor           = RedFont;
        }
        cell.Style.Font.Bold = true;
    }

    private static void ApplyBorder(IXLRange range)
    {
        range.Style.Border.OutsideBorder      = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder       = XLBorderStyleValues.Hair;
        range.Style.Border.OutsideBorderColor = XLColor.FromHtml("#AAAAAA");
        range.Style.Border.InsideBorderColor  = XLColor.FromHtml("#DDDDDD");
    }

    private static string FormatStatus(string s) => s switch
    {
        "CompletedWithIssues" => "Issues Found",
        "InProgress"          => "In Progress",
        "InService"           => "In Service",
        _                     => s
    };

    private static byte[] ToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
