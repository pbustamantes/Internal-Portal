using MediatR;

namespace InternalPortal.Application.Features.Events.Commands;

public record CancelEventCommand(Guid Id) : IRequest<Unit>;
