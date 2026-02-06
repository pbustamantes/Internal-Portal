using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Registrations.Commands;

public class CancelRegistrationCommandHandler : IRequestHandler<CancelRegistrationCommand, Unit>
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelRegistrationCommandHandler(IRegistrationRepository registrationRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _registrationRepository = registrationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CancelRegistrationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenException();

        var registration = await _registrationRepository.GetByUserAndEventAsync(userId, request.EventId, cancellationToken)
            ?? throw new NotFoundException("Registration", $"User:{userId}, Event:{request.EventId}");

        registration.Cancel();
        await _registrationRepository.UpdateAsync(registration, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
