using InternalPortal.Application.Features.Events.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public record GetUpcomingEventsQuery(int Count = 5) : IRequest<IReadOnlyList<EventSummaryDto>>;
