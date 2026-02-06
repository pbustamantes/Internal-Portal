namespace InternalPortal.Application.Features.Registrations.DTOs;

public record RegistrationDto(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid EventId,
    string EventTitle,
    string Status,
    DateTime RegisteredAtUtc);
