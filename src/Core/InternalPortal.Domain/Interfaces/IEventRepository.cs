using InternalPortal.Domain.Entities;

namespace InternalPortal.Domain.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    Task<(IReadOnlyList<Event> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, Guid? categoryId = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetUpcomingAsync(int count, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
