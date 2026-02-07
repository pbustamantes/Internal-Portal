using MediatR;

namespace InternalPortal.Application.Features.Venues;

public record UpdateVenueCommand(
    Guid Id,
    string Name,
    int Capacity,
    string Street,
    string City,
    string State,
    string ZipCode,
    string? Building,
    string? Room) : IRequest<VenueDto>;
