using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Events.DTOs;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using InternalPortal.Domain.ValueObjects;
using MediatR;

namespace InternalPortal.Application.Features.Events.Commands;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, EventDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEventCommandHandler(IEventRepository eventRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventDto> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await _eventRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Event", request.Id);

        evt.EnsureModifiable();

        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        if (evt.OrganizerId != userId && _currentUserService.Role != UserRole.Admin.ToString())
            throw new ForbiddenException();

        Address? location = null;
        if (!string.IsNullOrEmpty(request.Street))
            location = new Address(request.Street, request.City ?? "", request.State ?? "", request.ZipCode ?? "", request.Building, request.Room);

        evt.Title = request.Title;
        evt.Description = request.Description;
        evt.Schedule = new DateTimeRange(request.StartUtc, request.EndUtc);
        evt.Capacity = new Capacity(request.MinAttendees, request.MaxAttendees);
        evt.Location = location;
        evt.Recurrence = request.Recurrence;
        evt.CategoryId = request.CategoryId;
        evt.VenueId = request.VenueId;
        evt.UpdatedAtUtc = DateTime.UtcNow;

        await _eventRepository.UpdateAsync(evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var attendees = evt.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
        return new EventDto(
            evt.Id, evt.Title, evt.Description,
            evt.Schedule.StartUtc, evt.Schedule.EndUtc,
            evt.Capacity.MinAttendees, evt.Capacity.MaxAttendees, attendees,
            evt.Status.ToString(), evt.Recurrence.ToString(),
            location?.Street, location?.City, location?.State, location?.ZipCode, location?.Building, location?.Room,
            evt.OrganizerId, evt.Organizer?.FullName ?? "",
            evt.CategoryId, evt.Category?.Name, evt.Category?.ColorHex,
            evt.VenueId, evt.Venue?.Name, evt.CreatedAtUtc);
    }
}
