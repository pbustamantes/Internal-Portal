using FluentValidation;

namespace InternalPortal.Application.Features.Venues;

public class CreateVenueCommandValidator : AbstractValidator<CreateVenueCommand>
{
    public CreateVenueCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.State).NotEmpty();
        RuleFor(x => x.ZipCode).NotEmpty();
    }
}
