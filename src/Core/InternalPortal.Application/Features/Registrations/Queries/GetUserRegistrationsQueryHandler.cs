using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Registrations.DTOs;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Registrations.Queries;

public class GetUserRegistrationsQueryHandler : IRequestHandler<GetUserRegistrationsQuery, IReadOnlyList<RegistrationDto>>
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserRegistrationsQueryHandler(IRegistrationRepository registrationRepository, ICurrentUserService currentUserService)
    {
        _registrationRepository = registrationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RegistrationDto>> Handle(GetUserRegistrationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new ForbiddenException();
        var registrations = await _registrationRepository.GetByUserIdAsync(userId, cancellationToken);

        return registrations.Select(r => new RegistrationDto(
            r.Id, r.UserId, r.User?.FullName ?? "", r.EventId, r.Event?.Title ?? "",
            r.Status.ToString(), r.RegisteredAtUtc)).ToList();
    }
}
