using InternalPortal.Domain.Entities;
using InternalPortal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InternalPortal.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(4000);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Recurrence).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(e => e.Schedule, s =>
        {
            s.Property(d => d.StartUtc).HasColumnName("StartUtc").IsRequired();
            s.Property(d => d.EndUtc).HasColumnName("EndUtc").IsRequired();
        });

        builder.OwnsOne(e => e.Capacity, c =>
        {
            c.Property(x => x.MinAttendees).HasColumnName("MinAttendees");
            c.Property(x => x.MaxAttendees).HasColumnName("MaxAttendees");
        });

        builder.OwnsOne(e => e.Location, l =>
        {
            l.Property(a => a.Street).HasColumnName("LocationStreet").HasMaxLength(200);
            l.Property(a => a.City).HasColumnName("LocationCity").HasMaxLength(100);
            l.Property(a => a.State).HasColumnName("LocationState").HasMaxLength(50);
            l.Property(a => a.ZipCode).HasColumnName("LocationZipCode").HasMaxLength(20);
            l.Property(a => a.Building).HasColumnName("LocationBuilding").HasMaxLength(100);
            l.Property(a => a.Room).HasColumnName("LocationRoom").HasMaxLength(50);
        });

        builder.HasOne(e => e.Organizer).WithMany(u => u.OrganizedEvents).HasForeignKey(e => e.OrganizerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Category).WithMany(c => c.Events).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Venue).WithMany(v => v.Events).HasForeignKey(e => e.VenueId).OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(e => e.DomainEvents);
    }
}
