using CountrySecure.Application.DTOs.Auth;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class LoginUserValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(512);
    }
}

public class LogoutRequestValidator : AbstractValidator<LogoutRequestDto>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(512);
    }
}
