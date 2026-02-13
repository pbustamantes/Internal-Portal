using System.ComponentModel.DataAnnotations;

namespace InternalPortal.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters long.")]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "InternalPortal";

    [Required]
    public string Audience { get; set; } = "InternalPortalUsers";

    [Range(0.1, 168)]
    public double ExpiryHours { get; set; } = 1;
}
