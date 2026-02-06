using InternalPortal.Domain.Common;
using InternalPortal.Domain.ValueObjects;

namespace InternalPortal.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
    public int Capacity { get; set; }

    private readonly List<Event> _events = new();
    public IReadOnlyCollection<Event> Events => _events.AsReadOnly();
}
