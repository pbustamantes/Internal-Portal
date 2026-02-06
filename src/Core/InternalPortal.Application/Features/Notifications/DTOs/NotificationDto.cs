namespace InternalPortal.Application.Features.Notifications.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    Guid? EventId,
    DateTime CreatedAtUtc);
