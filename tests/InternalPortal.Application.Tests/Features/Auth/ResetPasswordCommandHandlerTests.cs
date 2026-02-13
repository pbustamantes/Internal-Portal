using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Common.Security;
using InternalPortal.Application.Features.Auth.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Moq;

namespace InternalPortal.Application.Tests.Features.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private ResetPasswordCommandHandler CreateHandler() => new(
        _userRepo.Object, _identityService.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WithValidToken_ShouldUpdatePasswordAndClearResetFields()
    {
        var rawToken = "test-token-value";
        var hashedToken = TokenHasher.HashToken(rawToken);

        var user = User.Create("test@example.com", "oldHash", "John", "Doe");
        user.PasswordResetToken = hashedToken;
        user.PasswordResetTokenExpiresUtc = DateTime.UtcNow.AddHours(1);

        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _identityService.Setup(s => s.HashPassword("NewPassword123")).Returns("newHash");

        var handler = CreateHandler();
        await handler.Handle(new ResetPasswordCommand("test@example.com", rawToken, "NewPassword123"), CancellationToken.None);

        user.PasswordHash.Should().Be("newHash");
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresUtc.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldThrow()
    {
        var rawToken = "test-token-value";
        var hashedToken = TokenHasher.HashToken(rawToken);

        var user = User.Create("test@example.com", "oldHash", "John", "Doe");
        user.PasswordResetToken = hashedToken;
        user.PasswordResetTokenExpiresUtc = DateTime.UtcNow.AddHours(-1);

        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ResetPasswordCommand("test@example.com", rawToken, "NewPassword123"), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_WithWrongToken_ShouldThrow()
    {
        var user = User.Create("test@example.com", "oldHash", "John", "Doe");
        user.PasswordResetToken = TokenHasher.HashToken("correct-token");
        user.PasswordResetTokenExpiresUtc = DateTime.UtcNow.AddHours(1);

        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ResetPasswordCommand("test@example.com", "wrong-token", "NewPassword123"), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldThrow()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ResetPasswordCommand("unknown@example.com", "some-token", "NewPassword123"), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_WithMissingResetTokenOnUser_ShouldThrow()
    {
        var user = User.Create("test@example.com", "oldHash", "John", "Doe");
        // PasswordResetToken is null by default — no reset was requested

        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ResetPasswordCommand("test@example.com", "some-token", "NewPassword123"), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_WithNullExpiry_ShouldThrow()
    {
        var rawToken = "test-token-value";
        var user = User.Create("test@example.com", "oldHash", "John", "Doe");
        user.PasswordResetToken = TokenHasher.HashToken(rawToken);
        user.PasswordResetTokenExpiresUtc = null; // token hash matches but expiry is null

        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ResetPasswordCommand("test@example.com", rawToken, "NewPassword123"), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("*expired*");
    }
}
