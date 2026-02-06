using InternalPortal.Application.Features.Notifications.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Notifications.Queries;

public record GetUserNotificationsQuery(bool UnreadOnly = false) : IRequest<IReadOnlyList<NotificationDto>>;
