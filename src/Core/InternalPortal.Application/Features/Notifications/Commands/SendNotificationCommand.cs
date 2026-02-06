using InternalPortal.Domain.Enums;
using MediatR;

namespace InternalPortal.Application.Features.Notifications.Commands;

public record SendNotificationCommand(
    Guid UserId,
    string Title,
    string Message,
    NotificationType Type,
    Guid? EventId = null) : IRequest<Unit>;
