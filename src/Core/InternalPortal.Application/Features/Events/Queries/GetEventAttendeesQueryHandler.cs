using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Features.Events.DTOs;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public class GetEventAttendeesQueryHandler : IRequestHandler<GetEventAttendeesQuery, IReadOnlyList<AttendeeDto>>
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEventRepository _eventRepository;

    public GetEventAttendeesQueryHandler(IRegistrationRepository registrationRepository, IEventRepository eventRepository)
    {
        _registrationRepository = registrationRepository;
        _eventRepository = eventRepository;
    }

    public async Task<IReadOnlyList<AttendeeDto>> Handle(GetEventAttendeesQuery request, CancellationToken cancellationToken)
    {
        _ = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException("Event", request.EventId);

        var registrations = await _registrationRepository.GetByEventIdAsync(request.EventId, cancellationToken);

        return registrations.Select(r => new AttendeeDto(
            r.UserId,
            r.User?.FullName ?? "",
            r.User?.Email ?? "",
            r.User?.Department,
            r.Status.ToString(),
            r.RegisteredAtUtc,
            r.User?.ProfilePictureUrl)).ToList();
    }
}
