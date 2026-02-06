using InternalPortal.Domain.Common;

namespace InternalPortal.Domain.Entities;

public class EventCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ColorHex { get; set; } = "#3B82F6";

    private readonly List<Event> _events = new();
    public IReadOnlyCollection<Event> Events => _events.AsReadOnly();
}
