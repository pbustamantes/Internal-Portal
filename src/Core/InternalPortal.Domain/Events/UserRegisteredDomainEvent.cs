using InternalPortal.Domain.Common;
using InternalPortal.Domain.Enums;

namespace InternalPortal.Domain.Events;

public sealed class UserRegisteredDomainEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid EventId { get; }
    public RegistrationStatus Status { get; }

    public UserRegisteredDomainEvent(Guid userId, Guid eventId, RegistrationStatus status)
    {
        UserId = userId;
        EventId = eventId;
        Status = status;
    }
}
