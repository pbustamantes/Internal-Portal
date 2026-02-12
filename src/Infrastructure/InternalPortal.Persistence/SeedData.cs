using System.Reflection;
using System.Text.Json;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InternalPortal.Persistence;

public static class SeedData
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync()) return;

        var categoryData = ReadSeedData<List<CategoryData>>("categories.json");
        var venueData = ReadSeedData<List<VenueData>>("venues.json");
        var userData = ReadSeedData<List<UserData>>("users.json");
        var eventData = ReadSeedData<List<EventData>>("events.json");

        // Seed categories
        var categories = categoryData.Select(c => new EventCategory
        {
            Id = Guid.NewGuid(),
            Name = c.Name,
            Description = c.Description,
            ColorHex = c.ColorHex,
            CreatedAtUtc = DateTime.UtcNow
        }).ToArray();
        context.EventCategories.AddRange(categories);

        // Seed venues
        var venues = venueData.Select(v => new Venue
        {
            Id = Guid.NewGuid(),
            Name = v.Name,
            Capacity = v.Capacity,
            Address = new Address(v.Address.Street, v.Address.City, v.Address.State, v.Address.ZipCode, v.Address.Building, v.Address.Room),
            CreatedAtUtc = DateTime.UtcNow
        }).ToArray();
        context.Venues.AddRange(venues);

        // Seed users
        var users = userData.Select(u =>
            User.Create(u.Email, u.PasswordHash, u.FirstName, u.LastName, u.Department, Enum.Parse<UserRole>(u.Role))
        ).ToArray();
        foreach (var user in users) context.Users.Add(user);

        // Build lookups
        var categoryLookup = categories.ToDictionary(c => c.Name);
        var venueLookup = venues.ToDictionary(v => v.Name);
        var userByRole = users.ToDictionary(u => u.Role.ToString());

        // Seed events
        var now = DateTime.UtcNow.Date;

        var events = eventData.Select(e =>
        {
            var venue = venueLookup[e.VenueName];
            var start = NextWeekday(now.AddDays(e.DayOffset)).AddHours(e.StartHour).AddMinutes(e.StartMinute);
            var end = start.AddHours(e.DurationHours).AddMinutes(e.DurationMinutes);

            return new Event
            {
                Id = Guid.NewGuid(),
                Title = e.Title,
                Description = e.Description,
                Schedule = new DateTimeRange(start, end),
                Capacity = new Capacity(e.MinAttendees, e.MaxAttendees),
                Location = venue.Address,
                Status = Enum.Parse<EventStatus>(e.Status),
                Recurrence = Enum.Parse<RecurrencePattern>(e.Recurrence),
                OrganizerId = userByRole[e.OrganizerRole].Id,
                CategoryId = categoryLookup[e.CategoryName].Id,
                VenueId = venue.Id,
                CreatedAtUtc = DateTime.UtcNow
            };
        }).ToArray();
        context.Events.AddRange(events);

        await context.SaveChangesAsync();
    }

    private static DateTime NextWeekday(DateTime date)
    {
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            date = date.AddDays(1);
        return date;
    }

    private static T ReadSeedData<T>(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"InternalPortal.Persistence.SeedData.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        return JsonSerializer.Deserialize<T>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize '{resourceName}'.");
    }

    // DTO records for JSON deserialization
    private record CategoryData(string Name, string Description, string ColorHex);
    private record AddressData(string Street, string City, string State, string ZipCode, string Building, string Room);
    private record VenueData(string Name, int Capacity, AddressData Address);
    private record UserData(string Email, string PasswordHash, string FirstName, string LastName, string Department, string Role);
    private record EventData(
        string Title, string Description, string CategoryName, string VenueName, string OrganizerRole,
        int DayOffset, int StartHour, int StartMinute, int DurationHours, int DurationMinutes,
        int MinAttendees, int MaxAttendees, string Status, string Recurrence);
}
