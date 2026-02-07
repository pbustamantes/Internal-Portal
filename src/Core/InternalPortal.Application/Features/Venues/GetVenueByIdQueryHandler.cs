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
            .Where(v => v.Id == request.Id)
            .Select(v => new VenueDto(
                v.Id, v.Name, v.Capacity,
                v.Address.Street, v.Address.City, v.Address.State, v.Address.ZipCode,
                v.Address.Building, v.Address.Room))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Venue", request.Id);

        return venue;
    }
}
