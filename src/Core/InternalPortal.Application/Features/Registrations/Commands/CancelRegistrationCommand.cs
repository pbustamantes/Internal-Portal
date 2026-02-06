using MediatR;

namespace InternalPortal.Application.Features.Registrations.Commands;

public record CancelRegistrationCommand(Guid EventId) : IRequest<Unit>;
