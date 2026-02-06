using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace InternalPortal.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", new { title, message, timestamp = DateTime.UtcNow }, cancellationToken);
    }

    public async Task SendToAllAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All
            .SendAsync("ReceiveNotification", new { title, message, timestamp = DateTime.UtcNow }, cancellationToken);
    }
}
