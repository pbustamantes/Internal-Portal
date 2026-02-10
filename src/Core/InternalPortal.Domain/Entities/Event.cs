using InternalPortal.Domain.Common;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Events;
using InternalPortal.Domain.Exceptions;
using InternalPortal.Domain.ValueObjects;

namespace InternalPortal.Domain.Entities;

public class Event : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeRange Schedule { get; set; } = null!;
    public Capacity Capacity { get; set; } = null!;
    public Address? Location { get; set; }
    public EventStatus Status { get; set; }
    public RecurrencePattern Recurrence { get; set; }

    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    public Guid? CategoryId { get; set; }
    public EventCategory? Category { get; set; }

    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }

    private readonly List<Registration> _registrations = new();
    public IReadOnlyCollection<Registration> Registrations => _registrations.AsReadOnly();

    public bool IsInPast => Schedule.EndUtc < DateTime.UtcNow;

    public void Complete()
    {
        if (Status == EventStatus.Completed)
            throw new DomainException("Event is already completed.");

        if (Status == EventStatus.Cancelled)
            throw new DomainException("Cannot complete a cancelled event.");

        Status = EventStatus.Completed;
    }

    public void EnsureModifiable()
    {
        if (IsInPast)
            throw new DomainException("Past events cannot be modified.");

        if (Status == EventStatus.Completed)
            throw new DomainException("Completed events cannot be modified.");
    }

    public Registration Register(Guid userId)
    {
        if (IsInPast)
            throw new DomainException("Cannot register for past events.");

        if (Status != EventStatus.Published)
            throw new DomainException("Can only register for published events.");

        if (_registrations.Any(r => r.UserId == userId && r.Status != RegistrationStatus.Cancelled))
            throw new DomainException("User is already registered for this event.");

        var activeCount = _registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
        var isWaitlisted = Capacity.IsFull(activeCount);

        var registration = new Registration
        {
            EventId = Id,
            UserId = userId,
            Status = isWaitlisted ? RegistrationStatus.Waitlisted : RegistrationStatus.Confirmed,
            RegisteredAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        _registrations.Add(registration);
        AddDomainEvent(new UserRegisteredDomainEvent(userId, Id, registration.Status));

        return registration;
    }

    public void Publish()
    {
        if (IsInPast)
            throw new DomainException("Past events cannot be published.");

        if (Status != EventStatus.Draft)
            throw new DomainException("Only draft events can be published.");

        Status = EventStatus.Published;
        AddDomainEvent(new EventCreatedDomainEvent(Id, Title));
    }

    public void Cancel()
    {
        if (IsInPast)
            throw new DomainException("Past events cannot be cancelled.");

        if (Status == EventStatus.Cancelled)
            throw new DomainException("Event is already cancelled.");

        Status = EventStatus.Cancelled;
        AddDomainEvent(new EventCancelledDomainEvent(Id, Title));
    }
}
