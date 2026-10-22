using CountrySecure.Application.DTOs.Users;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Ingresá tu contraseña actual.")
            .MinimumLength(8).WithMessage("La contraseña actual tiene al menos 8 caracteres.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Ingresá la nueva contraseña.")
            .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La nueva contraseña debe incluir una mayúscula.")
            .Matches("[a-z]").WithMessage("La nueva contraseña debe incluir una minúscula.")
            .Matches("[0-9]").WithMessage("La nueva contraseña debe incluir un número.")
            .Matches(@"^[a-zA-Z0-9!@#$%^&*()_+=\[{\]};:<>|./?-]*$")
                .WithMessage("La nueva contraseña tiene caracteres no permitidos.");

        // Que la nueva contraseña sea distinta que la vieja
        RuleFor(x => x)
            .Must(x => x.NewPassword != x.CurrentPassword)
            .WithMessage("La nueva contraseña debe ser distinta de la actual.");
    }
}