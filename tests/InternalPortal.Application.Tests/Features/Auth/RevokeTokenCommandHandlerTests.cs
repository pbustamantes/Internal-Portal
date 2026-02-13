using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Common.Security;
using InternalPortal.Application.Features.Auth.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Moq;

namespace InternalPortal.Application.Tests.Features.Auth;

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    private RevokeTokenCommandHandler CreateHandler() => new(
        _refreshTokenRepo.Object, _unitOfWork.Object, _currentUserService.Object);

    [Fact]
    public async Task Handle_WithOwnToken_ShouldRevokeSuccessfully()
    {
        var userId = Guid.NewGuid();
        var rawToken = "valid-token";
        var token = new RefreshToken
        {
            TokenHash = TokenHasher.HashToken(rawToken),
            UserId = userId,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };

        _currentUserService.Setup(s => s.UserId).Returns(userId);
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync(TokenHasher.HashToken(rawToken), It.IsAny<CancellationToken>())).ReturnsAsync(token);

        var handler = CreateHandler();
        await handler.Handle(new RevokeTokenCommand(rawToken), CancellationToken.None);

        token.RevokedAtUtc.Should().NotBeNull();
        _refreshTokenRepo.Verify(r => r.UpdateAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAnotherUsersToken_ShouldThrowForbidden()
    {
        var tokenOwnerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var rawToken = "other-user-token";
        var token = new RefreshToken
        {
            TokenHash = TokenHasher.HashToken(rawToken),
            UserId = tokenOwnerId,
            ExpiresUtc = DateTime.UtcNow.AddDays(7)
        };

        _currentUserService.Setup(s => s.UserId).Returns(currentUserId);
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync(TokenHasher.HashToken(rawToken), It.IsAny<CancellationToken>())).ReturnsAsync(token);

        var handler = CreateHandler();
        var act = () => handler.Handle(new RevokeTokenCommand(rawToken), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _refreshTokenRepo.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldThrowApplicationException()
    {
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new RevokeTokenCommand("nonexistent-token"), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_ShouldThrowApplicationException()
    {
        var userId = Guid.NewGuid();
        var rawToken = "revoked-token";
        var token = new RefreshToken
        {
            TokenHash = TokenHasher.HashToken(rawToken),
            UserId = userId,
            ExpiresUtc = DateTime.UtcNow.AddDays(7),
            RevokedAtUtc = DateTime.UtcNow.AddHours(-1)
        };

        _currentUserService.Setup(s => s.UserId).Returns(userId);
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync(TokenHasher.HashToken(rawToken), It.IsAny<CancellationToken>())).ReturnsAsync(token);

        var handler = CreateHandler();
        var act = () => handler.Handle(new RevokeTokenCommand(rawToken), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Token is already revoked.");
    }
}
