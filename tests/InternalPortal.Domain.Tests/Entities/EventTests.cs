using Xunit;
using FluentAssertions;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Exceptions;
using InternalPortal.Domain.ValueObjects;

namespace InternalPortal.Domain.Tests.Entities;

public class EventTests
{
    private Event CreateTestEvent(EventStatus status = EventStatus.Published)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test Event",
            Schedule = new DateTimeRange(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2)),
            Capacity = new Capacity(0, 10),
            Status = status,
            OrganizerId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private Event CreatePastEvent(EventStatus status = EventStatus.Published)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = "Past Event",
            Schedule = new DateTimeRange(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1)),
            Capacity = new Capacity(0, 10),
            Status = status,
            OrganizerId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow.AddDays(-3)
        };
    }

    [Fact]
    public void Register_WhenPublished_ShouldCreateConfirmedRegistration()
    {
        var evt = CreateTestEvent();
        var userId = Guid.NewGuid();

        var registration = evt.Register(userId);

        registration.Should().NotBeNull();
        registration.UserId.Should().Be(userId);
        registration.Status.Should().Be(RegistrationStatus.Confirmed);
        evt.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Register_WhenNotPublished_ShouldThrow()
    {
        var evt = CreateTestEvent(EventStatus.Draft);
        var act = () => evt.Register(Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*published*");
    }

    [Fact]
    public void Register_WhenFull_ShouldCreateWaitlistedRegistration()
    {
        var evt = CreateTestEvent();
        evt.Capacity = new Capacity(0, 1);

        evt.Register(Guid.NewGuid()); // fills it
        var reg2 = evt.Register(Guid.NewGuid()); // waitlisted

        reg2.Status.Should().Be(RegistrationStatus.Waitlisted);
    }

    [Fact]
    public void Register_WhenAlreadyRegistered_ShouldThrow()
    {
        var evt = CreateTestEvent();
        var userId = Guid.NewGuid();
        evt.Register(userId);

        var act = () => evt.Register(userId);

        act.Should().Throw<DomainException>().WithMessage("*already registered*");
    }

    [Fact]
    public void Publish_WhenDraft_ShouldChangeStatusToPublished()
    {
        var evt = CreateTestEvent(EventStatus.Draft);

        evt.Publish();

        evt.Status.Should().Be(EventStatus.Published);
        evt.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldThrow()
    {
        var evt = CreateTestEvent(EventStatus.Published);
        var act = () => evt.Publish();

        act.Should().Throw<DomainException>().WithMessage("*draft*");
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var evt = CreateTestEvent();

        evt.Cancel();

        evt.Status.Should().Be(EventStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var evt = CreateTestEvent(EventStatus.Cancelled);
        var act = () => evt.Cancel();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsInPast_WhenEndUtcIsInPast_ShouldReturnTrue()
    {
        var evt = CreatePastEvent();
        evt.IsInPast.Should().BeTrue();
    }

    [Fact]
    public void IsInPast_WhenEndUtcIsInFuture_ShouldReturnFalse()
    {
        var evt = CreateTestEvent();
        evt.IsInPast.Should().BeFalse();
    }

    [Fact]
    public void Complete_WhenPublished_ShouldChangeStatusToCompleted()
    {
        var evt = CreateTestEvent(EventStatus.Published);
        evt.Complete();
        evt.Status.Should().Be(EventStatus.Completed);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var evt = CreateTestEvent(EventStatus.Completed);
        var act = () => evt.Complete();
        act.Should().Throw<DomainException>().WithMessage("*already completed*");
    }

    [Fact]
    public void Complete_WhenCancelled_ShouldThrow()
    {
        var evt = CreateTestEvent(EventStatus.Cancelled);
        var act = () => evt.Complete();
        act.Should().Throw<DomainException>().WithMessage("*cancelled*");
    }

    [Fact]
    public void EnsureModifiable_WhenPast_ShouldThrow()
    {
        var evt = CreatePastEvent();
        var act = () => evt.EnsureModifiable();
        act.Should().Throw<DomainException>().WithMessage("*Past events*");
    }

    [Fact]
    public void EnsureModifiable_WhenCompleted_ShouldThrow()
    {
        var evt = CreateTestEvent(EventStatus.Completed);
        var act = () => evt.EnsureModifiable();
        act.Should().Throw<DomainException>().WithMessage("*Completed events*");
    }

    [Fact]
    public void Publish_WhenPast_ShouldThrow()
    {
        var evt = CreatePastEvent(EventStatus.Draft);
        var act = () => evt.Publish();
        act.Should().Throw<DomainException>().WithMessage("*Past events*");
    }

    [Fact]
    public void Cancel_WhenPast_ShouldThrow()
    {
        var evt = CreatePastEvent();
        var act = () => evt.Cancel();
        act.Should().Throw<DomainException>().WithMessage("*Past events*");
    }

    [Fact]
    public void Register_WhenPast_ShouldThrow()
    {
        var evt = CreatePastEvent();
        var act = () => evt.Register(Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*past events*");
    }
}
