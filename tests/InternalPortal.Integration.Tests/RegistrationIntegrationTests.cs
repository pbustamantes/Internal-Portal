using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Registrations.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.ValueObjects;
using InternalPortal.Persistence;
using InternalPortal.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InternalPortal.Integration.Tests;

public class RegistrationIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public RegistrationIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_Registration_" + Guid.NewGuid())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterForEvent_ShouldPersistRegistration()
    {
        // Arrange: seed a user and a published event
        var user = User.Create("test@test.com", "hashedpw", "Test", "User");
        _context.Users.Add(user);

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Integration Test Event",
            Schedule = new DateTimeRange(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2)),
            Capacity = new Capacity(0, 50),
            Status = EventStatus.Published,
            OrganizerId = user.Id,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        // Arrange: set up handler with real repos
        var attendee = User.Create("attendee@test.com", "hashedpw", "Attendee", "User");
        _context.Users.Add(attendee);
        await _context.SaveChangesAsync();

        var eventRepo = new EventRepository(_context);
        var unitOfWork = new UnitOfWork(_context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(s => s.UserId).Returns(attendee.Id);

        var handler = new CreateRegistrationCommandHandler(eventRepo, currentUser.Object, unitOfWork);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(evt.Id), CancellationToken.None);

        // Assert
        result.Status.Should().Be("Confirmed");
        result.EventId.Should().Be(evt.Id);
        result.UserId.Should().Be(attendee.Id);

        var savedRegistration = await _context.Registrations.FirstOrDefaultAsync(r => r.EventId == evt.Id && r.UserId == attendee.Id);
        savedRegistration.Should().NotBeNull();
        savedRegistration!.Status.Should().Be(RegistrationStatus.Confirmed);
    }

    [Fact]
    public async Task RegisterForEvent_WhenFull_ShouldPersistAsWaitlisted()
    {
        // Arrange: seed a user and a published event with capacity 1
        var organizer = User.Create("organizer@test.com", "hashedpw", "Org", "User");
        _context.Users.Add(organizer);

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Full Event",
            Schedule = new DateTimeRange(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2)),
            Capacity = new Capacity(0, 1),
            Status = EventStatus.Published,
            OrganizerId = organizer.Id,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Events.Add(evt);

        var firstAttendee = User.Create("first@test.com", "hashedpw", "First", "User");
        _context.Users.Add(firstAttendee);
        await _context.SaveChangesAsync();

        // Fill the event with the first attendee
        var eventRepo = new EventRepository(_context);
        var unitOfWork = new UnitOfWork(_context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(s => s.UserId).Returns(firstAttendee.Id);

        var handler = new CreateRegistrationCommandHandler(eventRepo, currentUser.Object, unitOfWork);
        await handler.Handle(new CreateRegistrationCommand(evt.Id), CancellationToken.None);

        // Arrange: second attendee tries to register
        var secondAttendee = User.Create("second@test.com", "hashedpw", "Second", "User");
        _context.Users.Add(secondAttendee);
        await _context.SaveChangesAsync();

        currentUser.Setup(s => s.UserId).Returns(secondAttendee.Id);

        // Act
        var result = await handler.Handle(new CreateRegistrationCommand(evt.Id), CancellationToken.None);

        // Assert
        result.Status.Should().Be("Waitlisted");

        var savedRegistration = await _context.Registrations.FirstOrDefaultAsync(r => r.EventId == evt.Id && r.UserId == secondAttendee.Id);
        savedRegistration.Should().NotBeNull();
        savedRegistration!.Status.Should().Be(RegistrationStatus.Waitlisted);
    }
}
