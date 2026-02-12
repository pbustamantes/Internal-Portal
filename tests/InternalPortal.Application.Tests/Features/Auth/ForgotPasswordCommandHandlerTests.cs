using Xunit;
using FluentAssertions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Application.Features.Auth.Commands;
using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace InternalPortal.Application.Tests.Features.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IConfiguration> _configuration = new();

    private ForgotPasswordCommandHandler CreateHandler() => new(
        _userRepo.Object, _unitOfWork.Object, _emailService.Object, _configuration.Object);

    public ForgotPasswordCommandHandlerTests()
    {
        _configuration.Setup(c => c["FrontendUrl"]).Returns("http://localhost:3000");
    }

    [Fact]
    public async Task Handle_WithValidEmail_ShouldUpdateUserAndSendEmail()
    {
        var user = User.Create("test@example.com", "hashedpw", "John", "Doe");
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        await handler.Handle(new ForgotPasswordCommand("test@example.com"), CancellationToken.None);

        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        user.PasswordResetTokenExpiresUtc.Should().BeAfter(DateTime.UtcNow);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendEmailAsync(
            "test@example.com",
            It.IsAny<string>(),
            It.Is<string>(body => body.Contains("reset-password")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldSucceedSilently()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ForgotPasswordCommand("unknown@example.com"), CancellationToken.None);

        await act.Should().NotThrowAsync();
        _emailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldSucceedSilently()
    {
        var user = User.Create("inactive@example.com", "hashedpw", "Jane", "Doe");
        user.IsActive = false;
        _userRepo.Setup(r => r.GetByEmailAsync("inactive@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = CreateHandler();
        var act = () => handler.Handle(new ForgotPasswordCommand("inactive@example.com"), CancellationToken.None);

        await act.Should().NotThrowAsync();
        _emailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
