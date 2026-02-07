using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Registrations.DTOs;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Registrations.Commands;

public class CreateRegistrationCommandHandler : IRequestHandler<CreateRegistrationCommand, RegistrationDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRegistrationCommandHandler(IEventRepository eventRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegistrationDto> Handle(CreateRegistrationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenException();

        var evt = await _eventRepository.GetByIdWithDetailsAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException("Event", request.EventId);

        var registration = evt.Register(userId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegistrationDto(registration.Id, userId, "", evt.Id, evt.Title, registration.Status.ToString(), registration.RegisteredAtUtc);
    }
}
