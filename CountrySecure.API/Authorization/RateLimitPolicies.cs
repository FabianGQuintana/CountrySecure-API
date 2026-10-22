namespace CountrySecure.API.Authorization
{
    public static class RateLimitPolicies
    {
        /// <summary>Recuperación de contraseña: pocos intentos por IP para evitar abusos.</summary>
        public const string PasswordReset = "password-reset";
    }
}
