using InternalPortal.Domain.Common;

namespace InternalPortal.Domain.Events;

public sealed class EventCancelledDomainEvent : BaseDomainEvent
{
    public Guid EventId { get; }
    public string Title { get; }

    public EventCancelledDomainEvent(Guid eventId, string title)
    {
        EventId = eventId;
        Title = title;
    }
}
