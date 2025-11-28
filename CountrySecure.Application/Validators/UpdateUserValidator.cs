using CountrySecure.Application.DTOs.Users;
using CountrySecure.Domain.Constants;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {

        RuleFor(x => x.Name)
            .MinimumLength(2)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Lastname)
            .MinimumLength(2)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Lastname));

        RuleFor(x => x.Dni)
            .InclusiveBetween(1000000, 99999999)
            .When(x => x.Dni.HasValue);

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]+$")
            .MinimumLength(7)
            .MaximumLength(15)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Role)
            .Must(role => RoleTypes.All.Contains(role))
            .When(x => !string.IsNullOrWhiteSpace(x.Role));

    }
}