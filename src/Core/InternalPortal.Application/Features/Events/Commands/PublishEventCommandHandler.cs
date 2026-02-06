using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Events.Commands;

public class PublishEventCommandHandler : IRequestHandler<PublishEventCommand, Unit>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public PublishEventCommandHandler(IEventRepository eventRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await _eventRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Event", request.Id);

        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        if (evt.OrganizerId != userId && _currentUserService.Role != UserRole.Admin.ToString())
            throw new ForbiddenException();

        evt.Publish();
        await _eventRepository.UpdateAsync(evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
