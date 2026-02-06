using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Persistence.Repositories;

public class RegistrationRepository : RepositoryBase<Registration>, IRegistrationRepository
{
    public RegistrationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await Context.Registrations
            .Include(r => r.User)
            .Include(r => r.Event)
            .Where(r => r.EventId == eventId)
            .OrderBy(r => r.RegisteredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Registration>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Registrations
            .Include(r => r.User)
            .Include(r => r.Event)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RegisteredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Registration?> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await Context.Registrations
            .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId, cancellationToken);
    }
}
