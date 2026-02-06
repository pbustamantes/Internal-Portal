using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Notifications.Commands;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Unit>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public SendNotificationCommandHandler(
        INotificationRepository notificationRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            EventId = request.EventId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.SendNotificationAsync(request.UserId, request.Title, request.Message, cancellationToken);

        return Unit.Value;
    }
}
