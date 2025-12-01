using CountrySecure.Application.DTOs.Users;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required")
            .MinimumLength(8).WithMessage("Current password must be at least 8 characters long");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
            .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("New password must contain at least one number")
            .Matches(@"^[a-zA-Z0-9!@#$%^&*()_+=\[{\]};:<>|./?-]*$")
                .WithMessage("New password contains invalid characters");

        // Que la nueva contraseña sea distinta que la vieja
        RuleFor(x => x)
            .Must(x => x.NewPassword != x.CurrentPassword)
            .WithMessage("New password must be different from the current password");
    }
}