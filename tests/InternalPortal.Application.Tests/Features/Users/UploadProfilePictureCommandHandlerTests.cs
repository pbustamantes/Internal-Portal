using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Users.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Moq;

namespace InternalPortal.Application.Tests.Features.Users;

public class UploadProfilePictureCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();

    private UploadProfilePictureCommandHandler CreateHandler() =>
        new(_currentUserService.Object, _userRepo.Object, _unitOfWork.Object, _fileStorage.Object);

    [Fact]
    public async Task Handle_WithValidFile_ShouldSaveAndReturnUrl()
    {
        var user = User.Create("test@test.com", "hash", "John", "Doe");
        _currentUserService.Setup(s => s.UserId).Returns(user.Id);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _fileStorage.Setup(f => f.SaveProfilePictureAsync(user.Id, ".png", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync($"/uploads/profile-pictures/{user.Id}.png");

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var handler = CreateHandler();
        var result = await handler.Handle(new UploadProfilePictureCommand(".png", stream), CancellationToken.None);

        result.ProfilePictureUrl.Should().Contain($"{user.Id}.png");
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingPicture_ShouldDeleteOldFirst()
    {
        var user = User.Create("test@test.com", "hash", "John", "Doe");
        user.ProfilePictureUrl = "/uploads/profile-pictures/old.png";
        _currentUserService.Setup(s => s.UserId).Returns(user.Id);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _fileStorage.Setup(f => f.SaveProfilePictureAsync(user.Id, ".jpg", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync($"/uploads/profile-pictures/{user.Id}.jpg");

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var handler = CreateHandler();
        await handler.Handle(new UploadProfilePictureCommand(".jpg", stream), CancellationToken.None);

        _fileStorage.Verify(f => f.DeleteProfilePicture("/uploads/profile-pictures/old.png"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoUserId_ShouldThrowForbidden()
    {
        _currentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        using var stream = new MemoryStream();
        var handler = CreateHandler();
        var act = () => handler.Handle(new UploadProfilePictureCommand(".png", stream), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
