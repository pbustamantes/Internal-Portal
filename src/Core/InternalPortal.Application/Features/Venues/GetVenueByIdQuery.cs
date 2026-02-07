using MediatR;

namespace InternalPortal.Application.Features.Venues;

public record GetVenueByIdQuery(Guid Id) : IRequest<VenueDto>;
