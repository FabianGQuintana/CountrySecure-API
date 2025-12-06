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
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2)
            .MaximumLength(50);

        RuleFor(x => x.Lastname)
            .NotEmpty().WithMessage("Lastname is required")
            .MinimumLength(2)
            .MaximumLength(50);

        RuleFor(x => x.Dni)
            .NotEmpty().WithMessage("Dni is required")
            .InclusiveBetween(1000000, 60000000)
            .WithMessage("Dni must be between 1,000,000 and 60,000,000");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^[0-9]+$").WithMessage("Phone must contain only numbers")
            .MinimumLength(7)
            .MaximumLength(15);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"^[a-zA-Z0-9!@#$%^&*()_+=\[{\]};:<>|./?-]*$")
                .WithMessage("Password contains invalid characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(role => RoleTypes.All.Contains(role))
            .WithMessage("Invalid role");
    }
}