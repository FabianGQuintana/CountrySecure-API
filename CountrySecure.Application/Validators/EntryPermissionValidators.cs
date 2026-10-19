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
        RuleFor(x => x.OrderId).NotEqual(Guid.Empty).When(x => x.OrderId.HasValue);
        RuleFor(x => x.ValidTo).GreaterThan(x => x.ValidFrom!.Value)
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
            .WithMessage("ValidTo must be after ValidFrom.");
    }
}

public class RegisterMovementValidator : AbstractValidator<RegisterMovementDto>
{
    public RegisterMovementValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}
