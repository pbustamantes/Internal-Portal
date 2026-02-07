using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Interfaces;
using InternalPortal.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Application.Features.Venues;

public class UpdateVenueCommandHandler : IRequestHandler<UpdateVenueCommand, VenueDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVenueCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<VenueDto> Handle(UpdateVenueCommand request, CancellationToken cancellationToken)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Venue", request.Id);

        venue.Name = request.Name;
        venue.Capacity = request.Capacity;
        venue.Address = new Address(request.Street, request.City, request.State, request.ZipCode, request.Building, request.Room);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VenueDto(
            venue.Id, venue.Name, venue.Capacity,
            venue.Address.Street, venue.Address.City, venue.Address.State, venue.Address.ZipCode,
            venue.Address.Building, venue.Address.Room);
    }
}
