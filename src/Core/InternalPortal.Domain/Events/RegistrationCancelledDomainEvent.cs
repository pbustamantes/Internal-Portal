using InternalPortal.Domain.Common;

namespace InternalPortal.Domain.Events;

public sealed class RegistrationCancelledDomainEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid EventId { get; }

    public RegistrationCancelledDomainEvent(Guid userId, Guid eventId)
    {
        UserId = userId;
        EventId = eventId;
    }
}
