using MediatR;

namespace InternalPortal.Application.Features.Venues;

public record GetVenuesQuery : IRequest<IReadOnlyList<VenueDto>>;
