using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using InternalPortal.Domain.ValueObjects;
using MediatR;

namespace InternalPortal.Application.Features.Venues;

public class CreateVenueCommandHandler : IRequestHandler<CreateVenueCommand, VenueDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVenueCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<VenueDto> Handle(CreateVenueCommand request, CancellationToken cancellationToken)
    {
        var venue = new Venue
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Capacity = request.Capacity,
            Address = new Address(request.Street, request.City, request.State, request.ZipCode, request.Building, request.Room)
        };

        _context.Venues.Add(venue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VenueDto(
            venue.Id, venue.Name, venue.Capacity,
            venue.Address.Street, venue.Address.City, venue.Address.State, venue.Address.ZipCode,
            venue.Address.Building, venue.Address.Room);
    }
}
