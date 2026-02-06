using InternalPortal.Application.Features.Registrations.DTOs;
using InternalPortal.Domain.Interfaces;
using MediatR;

namespace InternalPortal.Application.Features.Registrations.Queries;

public class GetRegistrationsByEventQueryHandler : IRequestHandler<GetRegistrationsByEventQuery, IReadOnlyList<RegistrationDto>>
{
    private readonly IRegistrationRepository _registrationRepository;

    public GetRegistrationsByEventQueryHandler(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<IReadOnlyList<RegistrationDto>> Handle(GetRegistrationsByEventQuery request, CancellationToken cancellationToken)
    {
        var registrations = await _registrationRepository.GetByEventIdAsync(request.EventId, cancellationToken);

        return registrations.Select(r => new RegistrationDto(
            r.Id, r.UserId, r.User?.FullName ?? "", r.EventId, r.Event?.Title ?? "",
            r.Status.ToString(), r.RegisteredAtUtc)).ToList();
    }
}
