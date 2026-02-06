using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InternalPortal.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync()) return;

        // Seed categories
        var categories = new[]
        {
            new EventCategory { Id = Guid.NewGuid(), Name = "Workshop", Description = "Hands-on learning sessions", ColorHex = "#3B82F6", CreatedAtUtc = DateTime.UtcNow },
            new EventCategory { Id = Guid.NewGuid(), Name = "Social", Description = "Team building and social events", ColorHex = "#10B981", CreatedAtUtc = DateTime.UtcNow },
            new EventCategory { Id = Guid.NewGuid(), Name = "Training", Description = "Professional development", ColorHex = "#F59E0B", CreatedAtUtc = DateTime.UtcNow },
            new EventCategory { Id = Guid.NewGuid(), Name = "Meeting", Description = "Company meetings", ColorHex = "#6366F1", CreatedAtUtc = DateTime.UtcNow },
            new EventCategory { Id = Guid.NewGuid(), Name = "Conference", Description = "Internal conferences", ColorHex = "#EC4899", CreatedAtUtc = DateTime.UtcNow }
        };
        context.EventCategories.AddRange(categories);

        // Seed venues
        var venues = new[]
        {
            new Venue { Id = Guid.NewGuid(), Name = "Main Auditorium", Address = new Address("100 Main St", "Austin", "TX", "78701", "HQ", "Auditorium"), Capacity = 500, CreatedAtUtc = DateTime.UtcNow },
            new Venue { Id = Guid.NewGuid(), Name = "Conference Room A", Address = new Address("100 Main St", "Austin", "TX", "78701", "HQ", "Room A"), Capacity = 30, CreatedAtUtc = DateTime.UtcNow },
            new Venue { Id = Guid.NewGuid(), Name = "Training Lab", Address = new Address("100 Main St", "Austin", "TX", "78701", "HQ", "Lab 1"), Capacity = 20, CreatedAtUtc = DateTime.UtcNow }
        };
        context.Venues.AddRange(venues);

        // Seed admin user (password: Admin123!)
        var admin = User.Create("admin@company.com", "$2a$11$K5mXjQh3GDjIF/EkzLc7xeJFCN5VBgq.oRPr3Wm8dFRVqESvKfKR.", "Admin", "User", "IT", UserRole.Admin);
        context.Users.Add(admin);

        // Seed organizer user (password: Organizer123!)
        var organizer = User.Create("organizer@company.com", "$2a$11$K5mXjQh3GDjIF/EkzLc7xeJFCN5VBgq.oRPr3Wm8dFRVqESvKfKR.", "Event", "Organizer", "HR", UserRole.Organizer);
        context.Users.Add(organizer);

        // Seed employee user (password: Employee123!)
        var employee = User.Create("employee@company.com", "$2a$11$K5mXjQh3GDjIF/EkzLc7xeJFCN5VBgq.oRPr3Wm8dFRVqESvKfKR.", "John", "Employee", "Engineering", UserRole.Employee);
        context.Users.Add(employee);

        await context.SaveChangesAsync();
    }
}
