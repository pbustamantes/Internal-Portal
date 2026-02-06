using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Events.DTOs;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using InternalPortal.Domain.ValueObjects;
using MediatR;

namespace InternalPortal.Application.Features.Events.Commands;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEventCommandHandler(IEventRepository eventRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenException();

        Address? location = null;
        if (!string.IsNullOrEmpty(request.Street))
            location = new Address(request.Street, request.City ?? "", request.State ?? "", request.ZipCode ?? "", request.Building, request.Room);

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Schedule = new DateTimeRange(request.StartUtc, request.EndUtc),
            Capacity = new Capacity(request.MinAttendees, request.MaxAttendees),
            Location = location,
            Status = EventStatus.Draft,
            Recurrence = request.Recurrence,
            OrganizerId = userId,
            CategoryId = request.CategoryId,
            VenueId = request.VenueId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _eventRepository.AddAsync(evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventDto(
            evt.Id, evt.Title, evt.Description,
            evt.Schedule.StartUtc, evt.Schedule.EndUtc,
            evt.Capacity.MinAttendees, evt.Capacity.MaxAttendees, 0,
            evt.Status.ToString(), evt.Recurrence.ToString(),
            location?.Street, location?.City, location?.State, location?.ZipCode, location?.Building, location?.Room,
            evt.OrganizerId, "", evt.CategoryId, null, null, evt.VenueId, null, evt.CreatedAtUtc);
    }
}
