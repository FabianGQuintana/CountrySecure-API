using CountrySecure.Application.DTOs.Lots;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class CreateLotValidator : AbstractValidator<CreateLotDto>
{
    public CreateLotValidator()
    {
        RuleFor(x => x.LotName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BlockName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LotState).IsInEnum();
    }
}

public class UpdateLotValidator : AbstractValidator<UpdateLotDto>
{
    public UpdateLotValidator()
    {
        RuleFor(x => x.LotName).NotEmpty().MaximumLength(100).When(x => x.LotName != null);
        RuleFor(x => x.BlockName).NotEmpty().MaximumLength(100).When(x => x.BlockName != null);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}
