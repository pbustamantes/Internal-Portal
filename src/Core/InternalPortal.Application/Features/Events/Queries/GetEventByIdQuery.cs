using InternalPortal.Application.Features.Events.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public record GetEventByIdQuery(Guid Id) : IRequest<EventDto>;
