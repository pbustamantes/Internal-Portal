using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Events.DTOs;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Events.Queries;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetEventByIdQueryHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var evt = await _eventRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Event", request.Id);

        if (evt.Status == EventStatus.Draft && _currentUserService.Role != UserRole.Admin.ToString())
            throw new ForbiddenException("Draft events are only visible to administrators.");

        if (evt.Status == EventStatus.Published && evt.IsInPast)
        {
            evt.Complete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var attendees = evt.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);

        return new EventDto(
            evt.Id, evt.Title, evt.Description,
            evt.Schedule.StartUtc, evt.Schedule.EndUtc,
            evt.Capacity.MinAttendees, evt.Capacity.MaxAttendees, attendees,
            evt.Status.ToString(), evt.Recurrence.ToString(),
            evt.Location?.Street, evt.Location?.City, evt.Location?.State, evt.Location?.ZipCode,
            evt.Location?.Building, evt.Location?.Room,
            evt.OrganizerId, evt.Organizer?.FullName ?? "",
            evt.CategoryId, evt.Category?.Name, evt.Category?.ColorHex,
            evt.VenueId, evt.Venue?.Name, evt.CreatedAtUtc);
    }
}
