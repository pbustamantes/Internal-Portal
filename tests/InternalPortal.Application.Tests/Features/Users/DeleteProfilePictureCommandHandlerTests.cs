using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Users.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Moq;

namespace InternalPortal.Application.Tests.Features.Users;

public class DeleteProfilePictureCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();

    private DeleteProfilePictureCommandHandler CreateHandler() =>
        new(_currentUserService.Object, _userRepo.Object, _unitOfWork.Object, _fileStorage.Object);

    [Fact]
    public async Task Handle_WithExistingPicture_ShouldDeleteAndClearUrl()
    {
        var user = User.Create("test@test.com", "hash", "John", "Doe");
        user.ProfilePictureUrl = "/uploads/profile-pictures/test.png";
        _currentUserService.Setup(s => s.UserId).Returns(user.Id);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        var result = await handler.Handle(new DeleteProfilePictureCommand(), CancellationToken.None);

        result.ProfilePictureUrl.Should().BeNull();
        _fileStorage.Verify(f => f.DeleteProfilePicture("/uploads/profile-pictures/test.png"), Times.Once);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoPicture_ShouldNotCallDeleteFile()
    {
        var user = User.Create("test@test.com", "hash", "John", "Doe");
        _currentUserService.Setup(s => s.UserId).Returns(user.Id);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        await handler.Handle(new DeleteProfilePictureCommand(), CancellationToken.None);

        _fileStorage.Verify(f => f.DeleteProfilePicture(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoUserId_ShouldThrowForbidden()
    {
        _currentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new DeleteProfilePictureCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
