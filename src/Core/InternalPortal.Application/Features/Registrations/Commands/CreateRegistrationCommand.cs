using InternalPortal.Application.Features.Registrations.DTOs;
using MediatR;

namespace InternalPortal.Application.Features.Registrations.Commands;

public record CreateRegistrationCommand(Guid EventId) : IRequest<RegistrationDto>;
