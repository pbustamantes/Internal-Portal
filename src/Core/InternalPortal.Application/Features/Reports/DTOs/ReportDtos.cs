namespace InternalPortal.Application.Features.Reports.DTOs;

public record EventAttendanceReportDto(
    Guid EventId,
    string EventTitle,
    DateTime StartUtc,
    int TotalRegistrations,
    int ConfirmedCount,
    int CancelledCount,
    int WaitlistedCount,
    double AttendanceRate);

public record MonthlyEventsReportDto(
    int Year,
    int Month,
    int TotalEvents,
    int TotalRegistrations,
    int AverageAttendees);

public record PopularEventDto(
    Guid EventId,
    string Title,
    string? CategoryName,
    int RegistrationCount,
    int MaxAttendees,
    double FillRate);
