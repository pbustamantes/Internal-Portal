using InternalPortal.Application.Features.Registrations.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Registrations.Queries;

public record GetUserRegistrationsQuery : IRequest<IReadOnlyList<RegistrationDto>>;
