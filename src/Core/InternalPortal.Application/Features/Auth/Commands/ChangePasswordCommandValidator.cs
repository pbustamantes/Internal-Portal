using FluentValidation;
using InternalPortal.Application.Common.Validation;

namespace InternalPortal.Application.Features.Auth.Commands;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).MustBeStrongPassword();
    }
}
