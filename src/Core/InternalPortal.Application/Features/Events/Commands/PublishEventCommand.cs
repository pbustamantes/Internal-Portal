using MediatR;

namespace InternalPortal.Application.Features.Events.Commands;

public record PublishEventCommand(Guid Id) : IRequest<Unit>;
