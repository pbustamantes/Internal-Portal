using InternalPortal.Domain.Common;
using InternalPortal.Domain.Enums;

namespace InternalPortal.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? EventId { get; set; }
    public Event? Event { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
