namespace InternalPortal.Application.Features.Events.DTOs;

public record EventDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartUtc,
    DateTime EndUtc,
    int MinAttendees,
    int MaxAttendees,
    int CurrentAttendees,
    string Status,
    string Recurrence,
    string? LocationStreet,
    string? LocationCity,
    string? LocationState,
    string? LocationZipCode,
    string? LocationBuilding,
    string? LocationRoom,
    Guid OrganizerId,
    string OrganizerName,
    Guid? CategoryId,
    string? CategoryName,
    string? CategoryColor,
    Guid? VenueId,
    string? VenueName,
    DateTime CreatedAtUtc);

public record EventSummaryDto(
    Guid Id,
    string Title,
    DateTime StartUtc,
    DateTime EndUtc,
    int MaxAttendees,
    int CurrentAttendees,
    string Status,
    string? CategoryName,
    string? CategoryColor,
    string OrganizerName);

public record AttendeeDto(
    Guid UserId,
    string FullName,
    string Email,
    string? Department,
    string RegistrationStatus,
    DateTime RegisteredAtUtc,
    string? ProfilePictureUrl = null);
