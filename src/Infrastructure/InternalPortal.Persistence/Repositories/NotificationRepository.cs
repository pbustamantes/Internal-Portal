using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Persistence.Repositories;

public class NotificationRepository : RepositoryBase<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, bool unreadOnly = false, CancellationToken cancellationToken = default)
    {
        var query = Context.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query.OrderByDescending(n => n.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }
}
