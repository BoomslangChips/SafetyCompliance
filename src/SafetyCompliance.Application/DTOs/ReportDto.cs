namespace SafetyCompliance.Application.DTOs;

// ═══════════════════════════════════════════════════════════════
//  SUMMARY TAB  — one card per plant, current-month snapshot
// ═══════════════════════════════════════════════════════════════

public record PlantReportSummaryDto(
    int      PlantId,
    string   PlantName,
    string   CompanyName,
    int      InspectionsCompleted,
    int      InspectionsTotal,
    int      EquipmentInspected,
    int      TotalEquipment,
    int      OpenIncidents,
    int      CriticalIncidents,
    int      ActiveServiceBookings,
    int      NotesCount,
    int      CompliancePct,
    DateOnly? LastInspectionDate);

// ═══════════════════════════════════════════════════════════════
//  MONTHLY REPORT  — detailed row-level DTOs
// ═══════════════════════════════════════════════════════════════

public record ReportRoundRowDto(
    int       Id,
    DateOnly  Date,
    string    Status,
    string    InspectorName,
    int       CompletedEquipment,
    int       TotalEquipment,
    int       FailedChecks,
    int       TotalChecks,
    DateTime? CompletedAt);

public record ReportEquipmentRowDto(
    int          RoundId,
    int          EquipmentId,
    string       Identifier,
    string       TypeName,
    string       SectionName,
    bool         IsComplete,
    int          PassChecks,
    int          FailChecks,
    List<string> FailedItems,
    string?      Comments);

public record ReportIssueRowDto(
    int       Id,
    string    Title,
    string    Priority,
    string    Status,
    string?   AssignedTo,
    DateOnly? DueDate,
    string?   EquipmentIdentifier,
    DateTime  CreatedAt,
    DateTime? ResolvedAt);

public record ReportServiceRowDto(
    int       Id,
    string    EquipmentIdentifier,
    string    TypeName,
    string    SectionName,
    string    Provider,
    string    Reason,
    string    Status,
    DateOnly  SentDate,
    DateOnly? ExpectedReturn,
    DateOnly? ActualReturn);

public record ReportNoteRowDto(
    int      Id,
    string   Title,
    string   Content,
    string   Category,
    string   Priority,
    bool     IsPinned,
    string?  EquipmentIdentifier,
    DateTime CreatedAt);

public record MonthlyReportDto(
    int     PlantId,
    string  PlantName,
    string  CompanyName,
    string? ContactName,
    string? ContactPhone,
    int     Year,
    int     Month,
    int     TotalEquipmentInPlant,
    int     CompliancePct,
    List<ReportRoundRowDto>     Rounds,
    List<ReportEquipmentRowDto> Equipment,
    List<ReportIssueRowDto>     Issues,
    List<ReportServiceRowDto>   ServiceBookings,
    List<ReportNoteRowDto>      Notes,
    DateTime GeneratedAt);
