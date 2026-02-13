using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Common.Security;
using InternalPortal.Application.Features.Auth.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Moq;

namespace InternalPortal.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IIdentityService> _identityService = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private LoginCommandHandler CreateHandler() => new(
        _userRepo.Object, _identityService.Object, _jwtService.Object,
        _refreshTokenRepo.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        var user = User.Create("test@example.com", "hashedpw", "John", "Doe");
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _identityService.Setup(s => s.VerifyPassword("password", "hashedpw")).Returns(true);
        _jwtService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("jwt-token");
        _jwtService.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");

        var handler = CreateHandler();
        var result = await handler.Handle(new LoginCommand("test@example.com", "password"), CancellationToken.None);

        result.AccessToken.Should().Be("jwt-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Email.Should().Be("test@example.com");

        // Verify stored token is hashed, not raw
        _refreshTokenRepo.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(t => t.TokenHash == TokenHasher.HashToken("refresh-token")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldThrow()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new LoginCommand("wrong@example.com", "password"), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldThrow()
    {
        var user = User.Create("test@example.com", "hashedpw", "John", "Doe");
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _identityService.Setup(s => s.VerifyPassword("wrong", "hashedpw")).Returns(false);

        var handler = CreateHandler();
        var act = () => handler.Handle(new LoginCommand("test@example.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>();
    }
}
