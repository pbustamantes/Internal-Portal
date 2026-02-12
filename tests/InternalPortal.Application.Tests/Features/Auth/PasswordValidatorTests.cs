using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using InternalPortal.Application.Features.Auth.Commands;

namespace InternalPortal.Application.Tests.Features.Auth;

public class RegisterUserCommandPasswordValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void ValidPassword_ShouldPassValidation()
    {
        var command = new RegisterUserCommand("test@example.com", "StrongP1ss", "John", "Doe", null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("", "Password is required.")]
    [InlineData("Sh0rt", "Password must be at least 8 characters.")]
    [InlineData("alllowercase1", "Password must contain at least one uppercase letter.")]
    [InlineData("ALLUPPERCASE1", "Password must contain at least one lowercase letter.")]
    [InlineData("NoDigitsHere", "Password must contain at least one digit.")]
    public void InvalidPassword_ShouldFailValidation(string password, string expectedMessage)
    {
        var command = new RegisterUserCommand("test@example.com", password, "John", "Doe", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void TooLongPassword_ShouldFailValidation()
    {
        var password = "A1" + new string('a', 127);
        var command = new RegisterUserCommand("test@example.com", password, "John", "Doe", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must not exceed 128 characters.");
    }
}

public class ResetPasswordCommandPasswordValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public void ValidPassword_ShouldPassValidation()
    {
        var command = new ResetPasswordCommand("test@example.com", "valid-token", "StrongP1ss");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Theory]
    [InlineData("", "Password is required.")]
    [InlineData("Sh0rt", "Password must be at least 8 characters.")]
    [InlineData("alllowercase1", "Password must contain at least one uppercase letter.")]
    [InlineData("ALLUPPERCASE1", "Password must contain at least one lowercase letter.")]
    [InlineData("NoDigitsHere", "Password must contain at least one digit.")]
    public void InvalidPassword_ShouldFailValidation(string password, string expectedMessage)
    {
        var command = new ResetPasswordCommand("test@example.com", "valid-token", password);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void TooLongPassword_ShouldFailValidation()
    {
        var password = "A1" + new string('a', 127);
        var command = new ResetPasswordCommand("test@example.com", "valid-token", password);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must not exceed 128 characters.");
    }
}

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public void ValidPassword_ShouldPassValidation()
    {
        var command = new ChangePasswordCommand("OldPassword1", "StrongP1ss");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void EmptyCurrentPassword_ShouldFailValidation()
    {
        var command = new ChangePasswordCommand("", "StrongP1ss");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Theory]
    [InlineData("", "Password is required.")]
    [InlineData("Sh0rt", "Password must be at least 8 characters.")]
    [InlineData("alllowercase1", "Password must contain at least one uppercase letter.")]
    [InlineData("ALLUPPERCASE1", "Password must contain at least one lowercase letter.")]
    [InlineData("NoDigitsHere", "Password must contain at least one digit.")]
    public void InvalidNewPassword_ShouldFailValidation(string password, string expectedMessage)
    {
        var command = new ChangePasswordCommand("OldPassword1", password);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void TooLongNewPassword_ShouldFailValidation()
    {
        var password = "A1" + new string('a', 127);
        var command = new ChangePasswordCommand("OldPassword1", password);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must not exceed 128 characters.");
    }
}
