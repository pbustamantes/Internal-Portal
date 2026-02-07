using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Application.Features.Venues;

public class DeleteVenueCommandHandler : IRequestHandler<DeleteVenueCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVenueCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteVenueCommand request, CancellationToken cancellationToken)
    {
        var venue = await _context.Venues
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Venue", request.Id);

        _context.Venues.Remove(venue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
