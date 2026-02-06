using InternalPortal.Domain.Entities;
using InternalPortal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalPortal.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Capacity).IsRequired();

        builder.OwnsOne(v => v.Address, a =>
        {
            a.Property(x => x.Street).HasColumnName("Street").HasMaxLength(200).IsRequired();
            a.Property(x => x.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            a.Property(x => x.State).HasColumnName("State").HasMaxLength(50).IsRequired();
            a.Property(x => x.ZipCode).HasColumnName("ZipCode").HasMaxLength(20).IsRequired();
            a.Property(x => x.Building).HasColumnName("Building").HasMaxLength(100);
            a.Property(x => x.Room).HasColumnName("Room").HasMaxLength(50);
        });

        builder.Ignore(v => v.DomainEvents);
    }
}
