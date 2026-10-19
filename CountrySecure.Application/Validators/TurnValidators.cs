using CountrySecure.Application.DTOs.Turns;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class CreateTurnValidator : AbstractValidator<CreateTurnDto>
{
    public CreateTurnValidator()
    {
        RuleFor(x => x.AmenityId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");
        RuleFor(x => x)
            .Must(x => x.EndTime - x.StartTime <= TimeSpan.FromHours(12))
            .WithMessage("A reservation cannot last more than 12 hours.")
            .When(x => x.EndTime > x.StartTime);
    }
}

public class UpdateTurnValidator : AbstractValidator<UpdateTurnDto>
{
    public UpdateTurnValidator()
    {
        RuleFor(x => x.TurnStatus).IsInEnum().When(x => x.TurnStatus.HasValue);
        RuleFor(x => x.AmenityId).NotEqual(Guid.Empty).When(x => x.AmenityId.HasValue);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime!.Value)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("End time must be after start time.");
    }
}
