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
            .WithMessage("La hora de fin debe ser posterior a la de inicio.");
        RuleFor(x => x)
            .Must(x => x.EndTime - x.StartTime <= TimeSpan.FromHours(12))
            .WithMessage("Una reserva no puede durar más de 12 horas.")
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
            .WithMessage("La hora de fin debe ser posterior a la de inicio.");
    }
}
