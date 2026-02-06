using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Registrations.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Enums;
using InternalPortal.Domain.Interfaces;
using MediatR;
using Moq;

namespace InternalPortal.Application.Tests.Features.Registrations;

public class CancelRegistrationCommandHandlerTests
{
    private readonly Mock<IRegistrationRepository> _registrationRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CancelRegistrationCommandHandler CreateHandler() =>
        new(_registrationRepo.Object, _currentUser.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCancelRegistration()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventId = eventId,
            Status = RegistrationStatus.Confirmed,
            RegisteredAtUtc = DateTime.UtcNow
        };

        _currentUser.Setup(s => s.UserId).Returns(userId);
        _registrationRepo.Setup(r => r.GetByUserAndEventAsync(userId, eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        var handler = CreateHandler();
        var result = await handler.Handle(new CancelRegistrationCommand(eventId), CancellationToken.None);

        result.Should().Be(Unit.Value);
        registration.Status.Should().Be(RegistrationStatus.Cancelled);
        _registrationRepo.Verify(r => r.UpdateAsync(registration, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRegistrationNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        _currentUser.Setup(s => s.UserId).Returns(userId);
        _registrationRepo.Setup(r => r.GetByUserAndEventAsync(userId, eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new CancelRegistrationCommand(eventId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowForbiddenException()
    {
        _currentUser.Setup(s => s.UserId).Returns((Guid?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new CancelRegistrationCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyCancelled_ShouldThrowDomainException()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventId = eventId,
            Status = RegistrationStatus.Cancelled,
            RegisteredAtUtc = DateTime.UtcNow
        };

        _currentUser.Setup(s => s.UserId).Returns(userId);
        _registrationRepo.Setup(r => r.GetByUserAndEventAsync(userId, eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registration);

        var handler = CreateHandler();
        var act = () => handler.Handle(new CancelRegistrationCommand(eventId), CancellationToken.None);

        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>();
    }
}
