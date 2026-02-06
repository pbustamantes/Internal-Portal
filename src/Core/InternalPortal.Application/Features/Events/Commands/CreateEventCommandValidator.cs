using FluentValidation;

namespace InternalPortal.Application.Features.Events.Commands;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.StartUtc).NotEmpty().GreaterThan(DateTime.UtcNow.AddMinutes(-5));
        RuleFor(x => x.EndUtc).NotEmpty().GreaterThan(x => x.StartUtc).WithMessage("End date must be after start date.");
        RuleFor(x => x.MinAttendees).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxAttendees).GreaterThanOrEqualTo(x => x.MinAttendees);
    }
}
