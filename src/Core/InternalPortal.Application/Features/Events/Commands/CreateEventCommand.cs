using InternalPortal.Application.Features.Events.DTOs;
using InternalPortal.Domain.Enums;
using MediatR;

namespace InternalPortal.Application.Features.Events.Commands;

public record CreateEventCommand(
    string Title,
    string? Description,
    DateTime StartUtc,
    DateTime EndUtc,
    int MinAttendees,
    int MaxAttendees,
    string? Street,
    string? City,
    string? State,
    string? ZipCode,
    string? Building,
    string? Room,
    RecurrencePattern Recurrence,
    Guid? CategoryId,
    Guid? VenueId) : IRequest<EventDto>;
