using InternalPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Event> Events { get; }
    DbSet<User> Users { get; }
    DbSet<Registration> Registrations { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EventCategory> EventCategories { get; }
    DbSet<Venue> Venues { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
