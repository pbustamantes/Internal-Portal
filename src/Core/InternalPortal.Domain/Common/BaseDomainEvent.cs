using MediatR;

namespace InternalPortal.Domain.Common;

public abstract class BaseDomainEvent : INotification
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
