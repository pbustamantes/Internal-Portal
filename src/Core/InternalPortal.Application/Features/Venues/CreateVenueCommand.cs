using MediatR;

namespace InternalPortal.Application.Features.Venues;

public record CreateVenueCommand(
    string Name,
    int Capacity,
    string Street,
    string City,
    string State,
    string ZipCode,
    string? Building,
    string? Room) : IRequest<VenueDto>;
