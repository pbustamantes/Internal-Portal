using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Events.Commands;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Unit>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEventCommandHandler(IEventRepository eventRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await _eventRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Event", request.Id);

        evt.EnsureModifiable();

        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        if (evt.OrganizerId != userId && _currentUserService.Role != UserRole.Admin.ToString())
            throw new ForbiddenException();

        await _eventRepository.DeleteAsync(evt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
