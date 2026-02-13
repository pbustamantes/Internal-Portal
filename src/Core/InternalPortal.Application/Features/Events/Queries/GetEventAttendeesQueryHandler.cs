using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Events.DTOs;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public class GetEventAttendeesQueryHandler : IRequestHandler<GetEventAttendeesQuery, IReadOnlyList<AttendeeDto>>
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetEventAttendeesQueryHandler(IRegistrationRepository registrationRepository, IEventRepository eventRepository, ICurrentUserService currentUserService)
    {
        _registrationRepository = registrationRepository;
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<AttendeeDto>> Handle(GetEventAttendeesQuery request, CancellationToken cancellationToken)
    {
        var evt = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException("Event", request.EventId);

        if (evt.Status == EventStatus.Draft && _currentUserService.Role != UserRole.Admin.ToString())
            throw new ForbiddenException("Draft events are only visible to administrators.");

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
