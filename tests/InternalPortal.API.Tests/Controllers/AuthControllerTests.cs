using Xunit;
using FluentAssertions;
using InternalPortal.API.Controllers;
using InternalPortal.Application.Features.Auth.Commands;
using InternalPortal.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InternalPortal.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task Login_WithValidCommand_ShouldReturnOk()
    {
        var authResponse = new AuthResponse("token", "refresh", DateTime.UtcNow.AddHours(1),
            new UserDto(Guid.NewGuid(), "test@test.com", "John", "Doe", null, "Employee"));

        _mediator.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var controller = new AuthController(_mediator.Object);
        var result = await controller.Login(new LoginCommand("test@test.com", "password"));

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(authResponse);
    }

    [Fact]
    public async Task Register_WithValidCommand_ShouldReturnOk()
    {
        var authResponse = new AuthResponse("token", "refresh", DateTime.UtcNow.AddHours(1),
            new UserDto(Guid.NewGuid(), "new@test.com", "Jane", "Doe", null, "Employee"));

        _mediator.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var controller = new AuthController(_mediator.Object);
        var result = await controller.Register(new RegisterUserCommand("new@test.com", "Password123", "Jane", "Doe", null));

        result.Should().BeOfType<OkObjectResult>();
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
