using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Events.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using Moq;

namespace InternalPortal.Application.Tests.Features.Events;

public class CreateEventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEvent()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(s => s.UserId).Returns(userId);
        _eventRepo.Setup(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event e, CancellationToken _) => e);

        var handler = new CreateEventCommandHandler(_eventRepo.Object, _currentUser.Object, _unitOfWork.Object);

        var result = await handler.Handle(new CreateEventCommand(
            "Test Event", "Description",
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2),
            0, 50, null, null, null, null, null, null,
            RecurrencePattern.None, null, null), CancellationToken.None);

        result.Title.Should().Be("Test Event");
        result.OrganizerId.Should().Be(userId);
        _eventRepo.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
