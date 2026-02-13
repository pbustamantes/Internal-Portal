using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Users.Queries;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Moq;

namespace InternalPortal.Application.Tests.Features.Users;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private GetCurrentUserQueryHandler CreateHandler() => new(_currentUserService.Object, _userRepo.Object);

    [Fact]
    public async Task Handle_WithAuthenticatedUser_ShouldReturnUserDto()
    {
        var user = User.Create("test@test.com", "hash", "John", "Doe", "Engineering");
        _currentUserService.Setup(s => s.UserId).Returns(user.Id);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.Email.Should().Be("test@test.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Department.Should().Be("Engineering");
    }

    [Fact]
    public async Task Handle_WithNoUserId_ShouldThrowForbidden()
    {
        _currentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserService.Setup(s => s.UserId).Returns(userId);
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
