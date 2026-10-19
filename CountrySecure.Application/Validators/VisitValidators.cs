using CountrySecure.Application.DTOs.Visits;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class CreateVisitValidator : AbstractValidator<CreateVisitDto>
{
    public CreateVisitValidator()
    {
        RuleFor(x => x.NameVisit).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.LastNameVisit).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.DniVisit).InclusiveBetween(1_000_000, 99_999_999)
            .WithMessage("DNI must be between 1,000,000 and 99,999,999.");
    }
}

public class UpdateVisitValidator : AbstractValidator<UpdateVisitDto>
{
    public UpdateVisitValidator()
    {
        RuleFor(x => x.NameVisit).NotEmpty().MinimumLength(2).MaximumLength(100).When(x => x.NameVisit != null);
        RuleFor(x => x.LastNameVisit).NotEmpty().MinimumLength(2).MaximumLength(100).When(x => x.LastNameVisit != null);
        RuleFor(x => x.DniVisit).InclusiveBetween(1_000_000, 99_999_999).When(x => x.DniVisit.HasValue);
    }
}
