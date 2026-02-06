using InternalPortal.Domain.Common;

namespace InternalPortal.Domain.Events;

public sealed class EventCreatedDomainEvent : BaseDomainEvent
{
    public Guid EventId { get; }
    public string Title { get; }

    public EventCreatedDomainEvent(Guid eventId, string title)
    {
        EventId = eventId;
        Title = title;
    }
}
