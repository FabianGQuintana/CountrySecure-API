using CountrySecure.Application.DTOs.EntryPermission;

namespace CountrySecure.Application.Interfaces.Services
{
    /// <summary>Genera el documento PDF del pase (QR en la esquina superior izquierda).</summary>
    public interface IPassDocumentService
    {
        byte[] GeneratePdf(PassDto pass);
    }
}
