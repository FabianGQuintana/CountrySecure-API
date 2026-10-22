namespace CountrySecure.Domain.Exceptions
{
    /// <summary>
    /// Credenciales o tokens inválidos, vencidos o revocados (HTTP 401).
    /// </summary>
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException(string message) : base(message) { }
    }
}
