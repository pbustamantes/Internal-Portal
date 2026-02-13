using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Users.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Moq;

namespace InternalPortal.Application.Tests.Features.Users;

public class UpdateProfileCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private UpdateProfileCommandHandler CreateHandler() => new(_currentUserService.Object, _userRepo.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateAndReturnUserDto()
    {
        var user = User.Create("test@test.com", "hash", "John", "Doe", "Engineering");
        _currentUserService.Setup(s => s.UserId).Returns(user.Id);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        var result = await handler.Handle(new UpdateProfileCommand("Jane", "Smith", "Marketing"), CancellationToken.None);

        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Department.Should().Be("Marketing");
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoUserId_ShouldThrowForbidden()
    {
        _currentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new UpdateProfileCommand("Jane", "Smith", null), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
