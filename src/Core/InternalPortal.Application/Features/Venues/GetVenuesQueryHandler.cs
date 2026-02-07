using InternalPortal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Application.Features.Venues;

public class GetVenuesQueryHandler : IRequestHandler<GetVenuesQuery, IReadOnlyList<VenueDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVenuesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VenueDto>> Handle(GetVenuesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Venues
            .OrderBy(v => v.Name)
            .Select(v => new VenueDto(v.Id, v.Name, v.Capacity))
            .ToListAsync(cancellationToken);
    }
}
