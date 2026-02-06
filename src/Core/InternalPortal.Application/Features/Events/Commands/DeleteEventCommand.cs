using MediatR;

namespace InternalPortal.Application.Features.Events.Commands;

public record DeleteEventCommand(Guid Id) : IRequest<Unit>;
