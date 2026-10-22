namespace CountrySecure.Application.DTOs.Common
{
    /// <summary>Bloquea o desbloquea (lista negra) a un visitante o a un residente.</summary>
    public class BlockDto
    {
        public bool Blocked { get; set; }
        public string? Reason { get; set; }
    }
}
