using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Application.Features.Venues;

public class GetVenueByIdQueryHandler : IRequestHandler<GetVenueByIdQuery, VenueDto>
{
    private readonly IApplicationDbContext _context;

    public GetVenueByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VenueDto> Handle(GetVenueByIdQuery request, CancellationToken cancellationToken)
    {
        var venue = await _context.Venues
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Venue", request.Id);

        return new VenueDto(
            venue.Id, venue.Name, venue.Capacity,
            venue.Address.Street, venue.Address.City, venue.Address.State, venue.Address.ZipCode,
            venue.Address.Building, venue.Address.Room);
    }
}
