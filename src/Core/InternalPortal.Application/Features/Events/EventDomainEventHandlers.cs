using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Events;
using MediatR;

namespace InternalPortal.Application.Features.Events;

public class EventCreatedDomainEventHandler : INotificationHandler<EventCreatedDomainEvent>
{
    private readonly INotificationService _notificationService;

    public EventCreatedDomainEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(EventCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.SendToAllAsync(
            "New Event Published",
            $"A new event '{notification.Title}' has been published!",
            cancellationToken);
    }
}

public class EventCancelledDomainEventHandler : INotificationHandler<EventCancelledDomainEvent>
{
    private readonly INotificationService _notificationService;

    public EventCancelledDomainEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(EventCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.SendToAllAsync(
            "Event Cancelled",
            $"The event '{notification.Title}' has been cancelled.",
            cancellationToken);
    }
}

public class UserRegisteredDomainEventHandler : INotificationHandler<UserRegisteredDomainEvent>
{
    private readonly INotificationService _notificationService;

    public UserRegisteredDomainEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        var statusMsg = notification.Status == Domain.Enums.RegistrationStatus.Waitlisted
            ? "You have been added to the waitlist."
            : "Your registration has been confirmed!";

        await _notificationService.SendNotificationAsync(
            notification.UserId,
            "Registration Update",
            statusMsg,
            cancellationToken);
    }
}
