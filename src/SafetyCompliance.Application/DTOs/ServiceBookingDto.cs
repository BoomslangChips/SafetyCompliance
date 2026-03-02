using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.DTOs;

public record ServiceBookingDto(
    int Id, int EquipmentId, string EquipmentIdentifier,
    string ServiceProvider, string Reason, ServiceBookingStatus Status,
    DateOnly SentDate, DateOnly? ExpectedReturnDate, DateOnly? ActualReturnDate,
    string? Notes, string CreatedById, DateTime CreatedAt);

public record ServiceBookingCreateDto(
    int EquipmentId, int? EquipmentInspectionId,
    string ServiceProvider, string Reason,
    DateOnly? ExpectedReturnDate, string? Notes);

public record ServiceBookingUpdateStatusDto(
    int Id, ServiceBookingStatus Status, DateOnly? ActualReturnDate, string? Notes);

public record ActiveServiceBookingDto(
    int Id, string ServiceProvider, ServiceBookingStatus Status,
    DateOnly SentDate, DateOnly? ExpectedReturnDate);

public record ServiceBookingOverviewDto(
    int Id, int EquipmentId, string EquipmentIdentifier, string EquipmentTypeName,
    string PlantName, string SectionName,
    string ServiceProvider, string Reason, ServiceBookingStatus Status,
    DateOnly SentDate, DateOnly? ExpectedReturnDate);
