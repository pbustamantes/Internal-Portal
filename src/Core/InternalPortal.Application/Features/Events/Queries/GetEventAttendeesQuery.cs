using InternalPortal.Application.Features.Events.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public record GetEventAttendeesQuery(Guid EventId) : IRequest<IReadOnlyList<AttendeeDto>>;
