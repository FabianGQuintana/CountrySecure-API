using CountrySecure.Application.DTOs.Properties;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class CreatePropertyValidator : AbstractValidator<CreatePropertyDto>
{
    public CreatePropertyValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PropertyNumber).GreaterThan(0).LessThanOrEqualTo(99999);
        RuleFor(x => x.LotId).NotEmpty();
    }
}

public class UpdatePropertyValidator : AbstractValidator<UpdatePropertyDto>
{
    public UpdatePropertyValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(150).When(x => x.Street != null);
        RuleFor(x => x.PropertyNumber).GreaterThan(0).LessThanOrEqualTo(99999).When(x => x.PropertyNumber.HasValue);
        RuleFor(x => x.PropertyStatus).IsInEnum().When(x => x.PropertyStatus.HasValue);
        RuleFor(x => x.UserId).NotEqual(Guid.Empty).When(x => x.UserId.HasValue);
        RuleFor(x => x.LotId).NotEqual(Guid.Empty).When(x => x.LotId.HasValue);
    }
}
