using MediatR;

namespace InternalPortal.Application.Features.Venues;

public record DeleteVenueCommand(Guid Id) : IRequest<Unit>;
