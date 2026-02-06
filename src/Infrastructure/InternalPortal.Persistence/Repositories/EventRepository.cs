using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Persistence.Repositories;

public class EventRepository : RepositoryBase<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Event> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Registrations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Title.Contains(search) || (e.Description != null && e.Description.Contains(search)));

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(e => e.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Event>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        return await Context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Registrations)
            .Where(e => e.Schedule.StartUtc >= startUtc && e.Schedule.EndUtc <= endUtc)
            .Where(e => e.Status == EventStatus.Published)
            .OrderBy(e => e.Schedule.StartUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetUpcomingAsync(int count, CancellationToken cancellationToken = default)
    {
        return await Context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Registrations)
            .Where(e => e.Schedule.StartUtc > DateTime.UtcNow && e.Status == EventStatus.Published)
            .OrderBy(e => e.Schedule.StartUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Event?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Include(e => e.Registrations)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
