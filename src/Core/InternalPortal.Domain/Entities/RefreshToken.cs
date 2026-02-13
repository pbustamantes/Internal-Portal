using InternalPortal.Domain.Common;

namespace InternalPortal.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string TokenHash { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime ExpiresUtc { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresUtc;
    public bool IsRevoked => RevokedAtUtc != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
