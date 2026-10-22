namespace CountrySecure.Application.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
