using Xunit;
using FluentAssertions;
using InternalPortal.API.Controllers;
using InternalPortal.Application.Features.Auth.Commands;
using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InternalPortal.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private AuthController CreateControllerWithCookies(string? refreshTokenCookie = null)
    {
        var controller = new AuthController(_mediator.Object);
        var httpContext = new DefaultHttpContext();
        if (refreshTokenCookie != null)
        {
            httpContext.Request.Headers["Cookie"] = $"refreshToken={refreshTokenCookie}";
        }
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public async Task Login_WithValidCommand_ShouldReturnOkAndSetRefreshCookie()
    {
        var authResponse = new AuthResponse("token", "refresh-value", DateTime.UtcNow.AddHours(1),
            new UserDto(Guid.NewGuid(), "test@test.com", "John", "Doe", null, "Employee"));

        _mediator.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var controller = CreateControllerWithCookies();
        var result = await controller.Login(new LoginCommand("test@test.com", "password"));

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(authResponse);

        // Verify refresh token cookie was set
        controller.HttpContext.Response.Headers["Set-Cookie"].ToString()
            .Should().Contain("refreshToken=refresh-value")
            .And.Contain("httponly")
            .And.Contain("samesite=strict")
            .And.Contain("path=/api/auth");
    }

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnOkAndSetRefreshCookie()
    {
        var authResponse = new AuthResponse("token", "refresh-value", DateTime.UtcNow.AddHours(1),
            new UserDto(Guid.NewGuid(), "new@test.com", "Jane", "Doe", null, "Employee"));

        _mediator.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var controller = CreateControllerWithCookies();
        var result = await controller.Register(new RegisterUserCommand("new@test.com", "Password123", "Jane", "Doe", null));

        result.Should().BeOfType<OkObjectResult>();
        controller.HttpContext.Response.Headers["Set-Cookie"].ToString()
            .Should().Contain("refreshToken=refresh-value");
    }

    [Fact]
    public async Task Refresh_WithCookie_ShouldReturnOkAndRotateCookie()
    {
        var authResponse = new AuthResponse("new-access", "new-refresh", DateTime.UtcNow.AddHours(1),
            new UserDto(Guid.NewGuid(), "test@test.com", "John", "Doe", null, "Employee"));

        _mediator.Setup(m => m.Send(It.Is<RefreshTokenCommand>(c => c.RefreshToken == "old-refresh"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var controller = CreateControllerWithCookies("old-refresh");
        var result = await controller.RefreshToken();

        result.Should().BeOfType<OkObjectResult>();
        controller.HttpContext.Response.Headers["Set-Cookie"].ToString()
            .Should().Contain("refreshToken=new-refresh");
    }

    [Fact]
    public async Task Refresh_WithoutCookie_ShouldReturn401()
    {
        var controller = CreateControllerWithCookies();
        var result = await controller.RefreshToken();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Revoke_WithCookie_ShouldClearCookieAndReturn204()
    {
        _mediator.Setup(m => m.Send(It.IsAny<RevokeTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var controller = CreateControllerWithCookies("some-refresh-token");
        var result = await controller.RevokeToken();

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Revoke_WithoutCookie_ShouldReturn400()
    {
        var controller = CreateControllerWithCookies();
        var result = await controller.RevokeToken();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldRevokeCookieTokenAndClearCookie()
    {
        _mediator.Setup(m => m.Send(It.IsAny<RevokeTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var controller = CreateControllerWithCookies("logout-refresh-token");
        var result = await controller.Logout();

        result.Should().BeOfType<NoContentResult>();
        _mediator.Verify(m => m.Send(It.Is<RevokeTokenCommand>(c => c.RefreshToken == "logout-refresh-token"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_WithoutCookie_ShouldStillReturn204()
    {
        var controller = CreateControllerWithCookies();
        var result = await controller.Logout();

        result.Should().BeOfType<NoContentResult>();
        _mediator.Verify(m => m.Send(It.IsAny<RevokeTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_WithValidCommand_ShouldReturnOk()
    {
        _mediator.Setup(m => m.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var controller = new AuthController(_mediator.Object);
        var result = await controller.ForgotPassword(new ForgotPasswordCommand("test@test.com"));

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ResetPassword_WithValidCommand_ShouldReturnOk()
    {
        _mediator.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var controller = new AuthController(_mediator.Object);
        var result = await controller.ResetPassword(new ResetPasswordCommand("test@test.com", "token", "NewPassword123"));

        result.Should().BeOfType<OkResult>();
    }
}
