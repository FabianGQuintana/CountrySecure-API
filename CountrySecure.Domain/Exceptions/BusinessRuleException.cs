namespace CountrySecure.Domain.Exceptions
{
    /// <summary>
    /// La solicitud es válida sintácticamente pero viola una regla de negocio (HTTP 422).
    /// Ej.: rango horario inválido, permiso fuera de vigencia, capacidad superada.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
    }
}
