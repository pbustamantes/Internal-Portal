using InternalPortal.Application.Features.Events.DTOs;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public class GetEventsByDateRangeQueryHandler : IRequestHandler<GetEventsByDateRangeQuery, IReadOnlyList<EventSummaryDto>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventsByDateRangeQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IReadOnlyList<EventSummaryDto>> Handle(GetEventsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.GetByDateRangeAsync(request.StartUtc, request.EndUtc, cancellationToken);

        return events.Select(e => new EventSummaryDto(
            e.Id, e.Title,
            e.Schedule.StartUtc, e.Schedule.EndUtc,
            e.Capacity.MaxAttendees,
            e.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed),
            e.Status.ToString(),
            e.Category?.Name, e.Category?.ColorHex,
            e.Organizer?.FullName ?? "")).ToList();
    }
}
