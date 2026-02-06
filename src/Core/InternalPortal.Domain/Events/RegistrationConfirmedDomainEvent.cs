using InternalPortal.Domain.Common;

namespace InternalPortal.Domain.Events;

public sealed class RegistrationConfirmedDomainEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid EventId { get; }

    public RegistrationConfirmedDomainEvent(Guid userId, Guid eventId)
    {
        UserId = userId;
        EventId = eventId;
    }
}
