namespace InternalPortal.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
    Task SendToAllAsync(string title, string message, CancellationToken cancellationToken = default);
}
