using InternalPortal.Domain.Entities;

namespace InternalPortal.Domain.Interfaces;

public interface IRegistrationRepository : IRepository<Registration>
{
    Task<IReadOnlyList<Registration>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Registration?> GetByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
}
