namespace CountrySecure.Domain.Exceptions
{
    /// <summary>
    /// El estado actual del recurso impide la operación (HTTP 409).
    /// Ej.: permiso cancelado o ya consumido, horario ya reservado.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
