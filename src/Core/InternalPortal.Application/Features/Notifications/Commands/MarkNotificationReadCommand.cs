using MediatR;

namespace InternalPortal.Application.Features.Notifications.Commands;

public record MarkNotificationReadCommand(Guid Id) : IRequest<Unit>;
