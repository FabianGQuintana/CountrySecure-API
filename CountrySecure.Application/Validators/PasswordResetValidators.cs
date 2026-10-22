using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.DTOs.Users;
using CountrySecure.Domain.Constants;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Ingresá tu email.")
            .MaximumLength(254)
            .EmailAddress().WithMessage("Ingresá un email válido.");
    }
}

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El enlace de recuperación no es válido.")
            .MaximumLength(128).WithMessage("El enlace de recuperación no es válido.");

        // Mismas reglas que el cambio de contraseña
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Ingresá la nueva contraseña.")
            .MinimumLength(8).WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128).WithMessage("La nueva contraseña no puede superar los 128 caracteres.")
            .Matches("[A-Z]").WithMessage("La nueva contraseña debe incluir una mayúscula.")
            .Matches("[a-z]").WithMessage("La nueva contraseña debe incluir una minúscula.")
            .Matches("[0-9]").WithMessage("La nueva contraseña debe incluir un número.")
            .Matches(@"^[a-zA-Z0-9!@#$%^&*()_+=\[{\]};:<>|./?-]*$")
                .WithMessage("La nueva contraseña tiene caracteres no permitidos.");
    }
}

public class UpdateAvatarValidator : AbstractValidator<UpdateAvatarDto>
{
    public UpdateAvatarValidator()
    {
        RuleFor(x => x.Avatar)
            .Must(a => a == null || Avatars.IsValid(a))
            .WithMessage("Elegí uno de los avatares disponibles.");
    }
}
