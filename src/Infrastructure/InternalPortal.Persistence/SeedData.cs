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

        // Seed events
        var now = DateTime.UtcNow.Date;

        static DateTime NextWeekday(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                date = date.AddDays(1);
            return date;
        }

        // Category lookup: Workshop=0, Social=1, Training=2, Meeting=3, Conference=4
        // Venue lookup: Main Auditorium=0, Conference Room A=1, Training Lab=2
        var events = new[]
        {
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "New Employee Onboarding",
                Description = "Comprehensive onboarding session for new hires covering company policies, tools, and culture. Bring your laptop and be ready to set up your development environment.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(3)).AddHours(9), NextWeekday(now.AddDays(3)).AddHours(12)),
                Capacity = new Capacity(8, 18),
                Location = venues[2].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = organizer.Id,
                CategoryId = categories[2].Id, // Training
                VenueId = venues[2].Id, // Training Lab
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Q1 All-Hands Meeting",
                Description = "Quarterly all-hands meeting to review company performance, celebrate wins, and discuss goals for the upcoming quarter. All employees are expected to attend.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(7)).AddHours(10), NextWeekday(now.AddDays(7)).AddHours(12)),
                Capacity = new Capacity(50, 200),
                Location = venues[0].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = admin.Id,
                CategoryId = categories[3].Id, // Meeting
                VenueId = venues[0].Id, // Main Auditorium
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "React & TypeScript Workshop",
                Description = "Hands-on workshop covering modern React patterns with TypeScript. Topics include hooks, context, generics, and building type-safe components. Prior JavaScript experience required.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(12)).AddHours(13), NextWeekday(now.AddDays(12)).AddHours(16)),
                Capacity = new Capacity(10, 25),
                Location = venues[1].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = organizer.Id,
                CategoryId = categories[0].Id, // Workshop
                VenueId = venues[1].Id, // Conference Room A
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Team Trivia Afternoon",
                Description = "A fun, casual trivia event to unwind and connect with colleagues. Form teams of 4-6 and compete for bragging rights and prizes. Snacks and drinks provided.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(17)).AddHours(16), NextWeekday(now.AddDays(17)).AddHours(17)),
                Capacity = new Capacity(50, 200),
                Location = venues[0].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = organizer.Id,
                CategoryId = categories[1].Id, // Social
                VenueId = venues[0].Id, // Main Auditorium
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Cloud Architecture Deep Dive",
                Description = "Full-day conference session exploring cloud architecture patterns including microservices, event-driven design, and infrastructure as code. Featuring talks from senior engineers.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(22)).AddHours(8), NextWeekday(now.AddDays(22)).AddHours(12)),
                Capacity = new Capacity(8, 18),
                Location = venues[2].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = admin.Id,
                CategoryId = categories[4].Id, // Conference
                VenueId = venues[2].Id, // Training Lab
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Effective Communication Skills",
                Description = "Interactive training on professional communication techniques including active listening, giving feedback, and presenting ideas clearly. Ideal for all experience levels.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(27)).AddHours(11), NextWeekday(now.AddDays(27)).AddHours(13)),
                Capacity = new Capacity(10, 25),
                Location = venues[1].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = organizer.Id,
                CategoryId = categories[2].Id, // Training
                VenueId = venues[1].Id, // Conference Room A
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Product Roadmap Review",
                Description = "Review of the product roadmap with leadership and stakeholders. We will discuss priorities, timelines, and resource allocation for the next planning cycle.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(32)).AddHours(14), NextWeekday(now.AddDays(32)).AddHours(15).AddMinutes(30)),
                Capacity = new Capacity(50, 200),
                Location = venues[0].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = admin.Id,
                CategoryId = categories[3].Id, // Meeting
                VenueId = venues[0].Id, // Main Auditorium
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Docker & Kubernetes Hands-On",
                Description = "Practical workshop on containerization with Docker and orchestration with Kubernetes. Participants will build, deploy, and manage containerized applications in a lab environment.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(37)).AddHours(9).AddMinutes(30), NextWeekday(now.AddDays(37)).AddHours(12).AddMinutes(30)),
                Capacity = new Capacity(8, 18),
                Location = venues[2].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = organizer.Id,
                CategoryId = categories[0].Id, // Workshop
                VenueId = venues[2].Id, // Training Lab
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Summer Kickoff Mixer",
                Description = "Celebrate the start of summer with food, music, and games. A great opportunity to meet people from other departments and enjoy an afternoon together.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(42)).AddHours(15), NextWeekday(now.AddDays(42)).AddHours(17)),
                Capacity = new Capacity(50, 200),
                Location = venues[0].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = organizer.Id,
                CategoryId = categories[1].Id, // Social
                VenueId = venues[0].Id, // Main Auditorium
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "API Design Best Practices",
                Description = "Conference session on designing robust and developer-friendly APIs. Topics include REST conventions, versioning strategies, error handling, and documentation with OpenAPI.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(47)).AddHours(10), NextWeekday(now.AddDays(47)).AddHours(13)),
                Capacity = new Capacity(10, 25),
                Location = venues[1].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = admin.Id,
                CategoryId = categories[4].Id, // Conference
                VenueId = venues[1].Id, // Conference Room A
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Leadership Development Program",
                Description = "A full morning of leadership training covering emotional intelligence, delegation, conflict resolution, and strategic thinking. Designed for current and aspiring team leads.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(52)).AddHours(8).AddMinutes(30), NextWeekday(now.AddDays(52)).AddHours(12).AddMinutes(30)),
                Capacity = new Capacity(50, 200),
                Location = venues[0].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = admin.Id,
                CategoryId = categories[2].Id, // Training
                VenueId = venues[0].Id, // Main Auditorium
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Sprint Retrospective Workshop",
                Description = "Learn and practice effective retrospective techniques beyond the standard format. Explore creative exercises to help teams reflect, learn, and continuously improve.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(57)).AddHours(13).AddMinutes(30), NextWeekday(now.AddDays(57)).AddHours(15).AddMinutes(30)),
                Capacity = new Capacity(10, 25),
                Location = venues[1].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = organizer.Id,
                CategoryId = categories[0].Id, // Workshop
                VenueId = venues[1].Id, // Conference Room A
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Cross-Team Collaboration Forum",
                Description = "An open forum for teams to share current projects, identify collaboration opportunities, and align on shared goals. Each team gets a 10-minute slot to present.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(62)).AddHours(11), NextWeekday(now.AddDays(62)).AddHours(12).AddMinutes(30)),
                Capacity = new Capacity(50, 200),
                Location = venues[0].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = admin.Id,
                CategoryId = categories[3].Id, // Meeting
                VenueId = venues[0].Id, // Main Auditorium
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Data Analytics with Python",
                Description = "Workshop on data analysis using Python, pandas, and visualization libraries. Participants will work through real datasets to build dashboards and extract insights.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(67)).AddHours(14), NextWeekday(now.AddDays(67)).AddHours(17)),
                Capacity = new Capacity(8, 18),
                Location = venues[2].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = organizer.Id,
                CategoryId = categories[0].Id, // Workshop
                VenueId = venues[2].Id, // Training Lab
                CreatedAtUtc = DateTime.UtcNow
            },
            new Event
            {
                Id = Guid.NewGuid(),
                Title = "Company Culture Town Hall",
                Description = "An open discussion on company values, culture initiatives, and employee engagement. Leadership will share updates and take questions from the floor.",
                Schedule = new DateTimeRange(NextWeekday(now.AddDays(72)).AddHours(10).AddMinutes(30), NextWeekday(now.AddDays(72)).AddHours(12).AddMinutes(30)),
                Capacity = new Capacity(50, 200),
                Location = venues[0].Address,
                Status = EventStatus.Published,
                Recurrence = RecurrencePattern.None,
                OrganizerId = admin.Id,
                CategoryId = categories[4].Id, // Conference
                VenueId = venues[0].Id, // Main Auditorium
                CreatedAtUtc = DateTime.UtcNow
            }
        };
        context.Events.AddRange(events);

        await context.SaveChangesAsync();
    }
}
