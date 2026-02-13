using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Events.Queries;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using InternalPortal.Domain.ValueObjects;
using Moq;

namespace InternalPortal.Application.Tests.Features.Events;

public class DraftEventVisibilityTests
{
    private readonly Mock<IEventRepository> _eventRepo = new();
    private readonly Mock<IRegistrationRepository> _registrationRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private static Event CreateEvent(EventStatus status)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = $"Test {status} Event",
            Description = "Description",
            Schedule = new DateTimeRange(DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(10).AddHours(2)),
            Capacity = new Capacity(0, 50),
            Status = status,
            OrganizerId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    // --- GetEventsQueryHandler ---

    [Fact]
    public async Task GetEvents_AsAdmin_ShouldIncludeDraftEvents()
    {
        var published = CreateEvent(EventStatus.Published);
        var draft = CreateEvent(EventStatus.Draft);
        var items = new List<Event> { published, draft };

        _eventRepo.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items as IReadOnlyList<Event>, 2));
        _currentUser.Setup(s => s.Role).Returns(UserRole.Admin.ToString());

        var handler = new GetEventsQueryHandler(_eventRepo.Object, _unitOfWork.Object, _currentUser.Object);
        var result = await handler.Handle(new GetEventsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(e => e.Status == "Draft");
        result.TotalCount.Should().Be(2);
    }

    [Theory]
    [InlineData("Employee")]
    [InlineData("Organizer")]
    public async Task GetEvents_AsNonAdmin_ShouldExcludeDraftEvents(string role)
    {
        var published = CreateEvent(EventStatus.Published);
        var draft = CreateEvent(EventStatus.Draft);
        var items = new List<Event> { published, draft };

        _eventRepo.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items as IReadOnlyList<Event>, 2));
        _currentUser.Setup(s => s.Role).Returns(role);

        var handler = new GetEventsQueryHandler(_eventRepo.Object, _unitOfWork.Object, _currentUser.Object);
        var result = await handler.Handle(new GetEventsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.Should().NotContain(e => e.Status == "Draft");
        result.TotalCount.Should().Be(1);
    }

    // --- GetEventByIdQueryHandler ---

    [Fact]
    public async Task GetEventById_DraftEvent_AsAdmin_ShouldReturnEvent()
    {
        var draft = CreateEvent(EventStatus.Draft);
        _eventRepo.Setup(r => r.GetByIdWithDetailsAsync(draft.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draft);
        _currentUser.Setup(s => s.Role).Returns(UserRole.Admin.ToString());

        var handler = new GetEventByIdQueryHandler(_eventRepo.Object, _unitOfWork.Object, _currentUser.Object);
        var result = await handler.Handle(new GetEventByIdQuery(draft.Id), CancellationToken.None);

        result.Title.Should().Be(draft.Title);
        result.Status.Should().Be("Draft");
    }

    [Theory]
    [InlineData("Employee")]
    [InlineData("Organizer")]
    public async Task GetEventById_DraftEvent_AsNonAdmin_ShouldThrowForbidden(string role)
    {
        var draft = CreateEvent(EventStatus.Draft);
        _eventRepo.Setup(r => r.GetByIdWithDetailsAsync(draft.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draft);
        _currentUser.Setup(s => s.Role).Returns(role);

        var handler = new GetEventByIdQueryHandler(_eventRepo.Object, _unitOfWork.Object, _currentUser.Object);
        var act = () => handler.Handle(new GetEventByIdQuery(draft.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Theory]
    [InlineData("Employee")]
    [InlineData("Organizer")]
    public async Task GetEventById_PublishedEvent_AsNonAdmin_ShouldReturnEvent(string role)
    {
        var published = CreateEvent(EventStatus.Published);
        _eventRepo.Setup(r => r.GetByIdWithDetailsAsync(published.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(published);
        _currentUser.Setup(s => s.Role).Returns(role);

        var handler = new GetEventByIdQueryHandler(_eventRepo.Object, _unitOfWork.Object, _currentUser.Object);
        var result = await handler.Handle(new GetEventByIdQuery(published.Id), CancellationToken.None);

        result.Title.Should().Be(published.Title);
        result.Status.Should().Be("Published");
    }

    // --- GetEventAttendeesQueryHandler ---

    [Fact]
    public async Task GetEventAttendees_DraftEvent_AsAdmin_ShouldReturnAttendees()
    {
        var draft = CreateEvent(EventStatus.Draft);
        _eventRepo.Setup(r => r.GetByIdAsync(draft.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draft);
        _registrationRepo.Setup(r => r.GetByEventIdAsync(draft.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Registration>());
        _currentUser.Setup(s => s.Role).Returns(UserRole.Admin.ToString());

        var handler = new GetEventAttendeesQueryHandler(_registrationRepo.Object, _eventRepo.Object, _currentUser.Object);
        var result = await handler.Handle(new GetEventAttendeesQuery(draft.Id), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Employee")]
    [InlineData("Organizer")]
    public async Task GetEventAttendees_DraftEvent_AsNonAdmin_ShouldThrowForbidden(string role)
    {
        var draft = CreateEvent(EventStatus.Draft);
        _eventRepo.Setup(r => r.GetByIdAsync(draft.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draft);
        _currentUser.Setup(s => s.Role).Returns(role);

        var handler = new GetEventAttendeesQueryHandler(_registrationRepo.Object, _eventRepo.Object, _currentUser.Object);
        var act = () => handler.Handle(new GetEventAttendeesQuery(draft.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
