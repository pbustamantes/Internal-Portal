using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Registrations.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using InternalPortal.Domain.ValueObjects;
using Moq;

namespace InternalPortal.Application.Tests.Features.Registrations;

public class CreateRegistrationCommandHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateRegistrationCommandHandler CreateHandler() =>
        new(_eventRepo.Object, _currentUser.Object, _unitOfWork.Object);

    private static Event CreatePublishedEvent(int maxAttendees = 50)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test Event",
            Schedule = new DateTimeRange(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2)),
            Capacity = new Capacity(0, maxAttendees),
            Status = EventStatus.Published,
            OrganizerId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnConfirmedRegistration()
    {
        var userId = Guid.NewGuid();
        var evt = CreatePublishedEvent();
        _currentUser.Setup(s => s.UserId).Returns(userId);
        _eventRepo.Setup(r => r.GetByIdWithDetailsAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        var handler = CreateHandler();
        var result = await handler.Handle(new CreateRegistrationCommand(evt.Id), CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.EventId.Should().Be(evt.Id);
        result.EventTitle.Should().Be("Test Event");
        result.Status.Should().Be("Confirmed");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEventIsFull_ShouldReturnWaitlistedRegistration()
    {
        var userId = Guid.NewGuid();
        var evt = CreatePublishedEvent(maxAttendees: 1);
        // Fill the event first
        evt.Register(Guid.NewGuid());

        _currentUser.Setup(s => s.UserId).Returns(userId);
        _eventRepo.Setup(r => r.GetByIdWithDetailsAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        var handler = CreateHandler();
        var result = await handler.Handle(new CreateRegistrationCommand(evt.Id), CancellationToken.None);

        result.Status.Should().Be("Waitlisted");
    }

    [Fact]
    public async Task Handle_WhenEventNotFound_ShouldThrowNotFoundException()
    {
        var eventId = Guid.NewGuid();
        _currentUser.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _eventRepo.Setup(r => r.GetByIdWithDetailsAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync((Event?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new CreateRegistrationCommand(eventId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowForbiddenException()
    {
        _currentUser.Setup(s => s.UserId).Returns((Guid?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new CreateRegistrationCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenEventNotPublished_ShouldThrowDomainException()
    {
        var userId = Guid.NewGuid();
        var evt = CreatePublishedEvent();
        evt.Status = EventStatus.Draft;

        _currentUser.Setup(s => s.UserId).Returns(userId);
        _eventRepo.Setup(r => r.GetByIdWithDetailsAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        var handler = CreateHandler();
        var act = () => handler.Handle(new CreateRegistrationCommand(evt.Id), CancellationToken.None);

        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyRegistered_ShouldThrowDomainException()
    {
        var userId = Guid.NewGuid();
        var evt = CreatePublishedEvent();
        evt.Register(userId); // register once

        _currentUser.Setup(s => s.UserId).Returns(userId);
        _eventRepo.Setup(r => r.GetByIdWithDetailsAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        var handler = CreateHandler();
        var act = () => handler.Handle(new CreateRegistrationCommand(evt.Id), CancellationToken.None);

        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>();
    }
}
