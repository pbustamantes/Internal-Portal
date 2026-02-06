using InternalPortal.Application.Features.Events.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public record GetEventsByDateRangeQuery(DateTime StartUtc, DateTime EndUtc) : IRequest<IReadOnlyList<EventSummaryDto>>;
