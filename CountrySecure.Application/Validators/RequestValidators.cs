using CountrySecure.Application.DTOs.Request;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class CreateRequestValidator : AbstractValidator<CreateRequestDto>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.Details).NotEmpty().MinimumLength(5).MaximumLength(500);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

public class UpdateRequestValidator : AbstractValidator<UpdateRequestDto>
{
    public UpdateRequestValidator()
    {
        RuleFor(x => x.Details).NotEmpty().MinimumLength(5).MaximumLength(500).When(x => x.Details != null);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300).When(x => x.Location != null);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}
