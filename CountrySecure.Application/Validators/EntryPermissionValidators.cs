using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Domain.Enums;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class CreateEntryPermissionValidator : AbstractValidator<CreateEntryPermissionDto>
{
    private const string PlatePattern = @"^[A-Za-z0-9 \-]{5,12}$";

    public CreateEntryPermissionValidator()
    {
        RuleFor(x => x.PermissionType).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.CompanionsCount).InclusiveBetween(0, 20);
        RuleFor(x => x.VehicleType).IsInEnum();
        RuleFor(x => x.PassType).IsInEnum();
        RuleFor(x => x.VehicleModel).MaximumLength(60);
        // Auto o moto: la patente es obligatoria (5 a 10 letras o números)
        RuleFor(x => x.VehiclePlate).NotEmpty()
            .When(x => x.VehicleType != VehicleType.OnFoot)
            .WithMessage("La patente es obligatoria si la visita llega en auto o moto.");
        RuleFor(x => x.VehiclePlate).Matches(PlatePattern)
            .When(x => x.VehicleType != VehicleType.OnFoot && !string.IsNullOrWhiteSpace(x.VehiclePlate))
            .WithMessage("La patente debe tener entre 5 y 10 letras o números.");
        RuleFor(x => x.ValidFrom).NotEmpty();
        RuleFor(x => x.ValidTo).NotEmpty().GreaterThan(x => x.ValidFrom)
            .WithMessage("La fecha de fin debe ser posterior a la de inicio.");
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        // Un permiso de mantenimiento debe indicar a qué servicio (orden) corresponde
        RuleFor(x => x.OrderId).NotEmpty()
            .When(x => x.PermissionType == PermissionType.Maintenance)
            .WithMessage("Elegí el servicio para un permiso de mantenimiento.");
    }
}

public class UpdateEntryPermissionValidator : AbstractValidator<UpdateEntryPermissionDto>
{
    public UpdateEntryPermissionValidator()
    {
        RuleFor(x => x.Description).MaximumLength(300);
        RuleFor(x => x.CompanionsCount).InclusiveBetween(0, 20).When(x => x.CompanionsCount.HasValue);
        RuleFor(x => x.VehicleType).IsInEnum().When(x => x.VehicleType.HasValue);
        RuleFor(x => x.PassType).IsInEnum().When(x => x.PassType.HasValue);
        RuleFor(x => x.VehicleModel).MaximumLength(60);
        RuleFor(x => x.VehiclePlate).MaximumLength(15);
        RuleFor(x => x.PermissionType).IsInEnum().When(x => x.PermissionType.HasValue);
        RuleFor(x => x.OrderId).NotEqual(Guid.Empty).When(x => x.OrderId.HasValue);
        RuleFor(x => x.ValidTo).GreaterThan(x => x.ValidFrom!.Value)
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
            .WithMessage("La fecha de fin debe ser posterior a la de inicio.");
    }
}

public class RegisterMovementValidator : AbstractValidator<RegisterMovementDto>
{
    public RegisterMovementValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}

public class ScanRequestValidator : AbstractValidator<ScanRequestDto>
{
    public ScanRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Action).IsInEnum();
    }
}
