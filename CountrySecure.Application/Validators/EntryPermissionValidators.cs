using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Domain.Enums;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class CreateEntryPermissionValidator : AbstractValidator<CreateEntryPermissionDto>
{
    public CreateEntryPermissionValidator()
    {
        RuleFor(x => x.PermissionType).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.ValidFrom).NotEmpty();
        RuleFor(x => x.ValidTo).NotEmpty().GreaterThan(x => x.ValidFrom)
            .WithMessage("ValidTo must be after ValidFrom.");
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        // Un permiso de mantenimiento debe indicar a qué servicio (orden) corresponde
        RuleFor(x => x.OrderId).NotEmpty()
            .When(x => x.PermissionType == PermissionType.Maintenance)
            .WithMessage("OrderId is required for maintenance permits.");
    }
}

public class UpdateEntryPermissionValidator : AbstractValidator<UpdateEntryPermissionDto>
{
    public UpdateEntryPermissionValidator()
    {
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.PermissionType).IsInEnum().When(x => x.PermissionType.HasValue);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.DepartureTime).GreaterThanOrEqualTo(x => x.EntryTime!.Value)
            .When(x => x.EntryTime.HasValue && x.DepartureTime.HasValue)
            .WithMessage("Departure time cannot be before entry time.");
    }
}

public class RegisterMovementValidator : AbstractValidator<RegisterMovementDto>
{
    public RegisterMovementValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}
