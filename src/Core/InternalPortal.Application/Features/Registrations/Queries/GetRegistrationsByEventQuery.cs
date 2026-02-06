using InternalPortal.Application.Features.Registrations.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Registrations.Queries;

public record GetRegistrationsByEventQuery(Guid EventId) : IRequest<IReadOnlyList<RegistrationDto>>;
