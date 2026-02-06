using InternalPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalPortal.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Token).IsRequired().HasMaxLength(512);
        builder.HasIndex(t => t.Token).IsUnique();
        builder.Property(t => t.CreatedByIp).HasMaxLength(50);
        builder.Property(t => t.RevokedByIp).HasMaxLength(50);
        builder.Property(t => t.ReplacedByToken).HasMaxLength(512);

        builder.HasOne(t => t.User).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsRevoked);
        builder.Ignore(t => t.IsActive);
        builder.Ignore(t => t.DomainEvents);
    }
}
