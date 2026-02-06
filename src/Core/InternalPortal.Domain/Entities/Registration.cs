using InternalPortal.Domain.Common;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Events;
using InternalPortal.Domain.Exceptions;

namespace InternalPortal.Domain.Entities;

public class Registration : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public RegistrationStatus Status { get; set; }
    public DateTime RegisteredAtUtc { get; set; }

    public void Confirm()
    {
        if (Status != RegistrationStatus.Pending && Status != RegistrationStatus.Waitlisted)
            throw new DomainException("Only pending or waitlisted registrations can be confirmed.");

        Status = RegistrationStatus.Confirmed;
        AddDomainEvent(new RegistrationConfirmedDomainEvent(UserId, EventId));
    }

    public void Cancel()
    {
        if (Status == RegistrationStatus.Cancelled)
            throw new DomainException("Registration is already cancelled.");

        Status = RegistrationStatus.Cancelled;
        AddDomainEvent(new RegistrationCancelledDomainEvent(UserId, EventId));
    }
}
