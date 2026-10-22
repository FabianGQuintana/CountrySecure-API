using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.DTOs.Users;
using CountrySecure.Domain.Constants;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ingresá el nombre.")
            .MinimumLength(2)
            .MaximumLength(50);

        RuleFor(x => x.Lastname)
            .NotEmpty().WithMessage("Ingresá el apellido.")
            .MinimumLength(2)
            .MaximumLength(50);

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("Ingresá el DNI.")
            .InclusiveBetween(1000000, 99999999)
            .WithMessage("El DNI debe estar entre 1.000.000 y 99.999.999.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Ingresá el teléfono.")
            .Matches(@"^[0-9]+$").WithMessage("El teléfono solo puede tener números.")
            .MinimumLength(7)
            .MaximumLength(15);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Ingresá el email.")
            .EmailAddress().WithMessage("El email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Ingresá una contraseña.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe incluir una mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe incluir una minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe incluir un número.")
            .Matches(@"^[a-zA-Z0-9!@#$%^&*()_+=\[{\]};:<>|./?-]*$")
                .WithMessage("La contraseña tiene caracteres no permitidos.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Elegí un rol.")
            .Must(role => RoleTypes.All.Contains(role))
            .WithMessage("El rol no es válido.");
    }
}