using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>
    /// Enlace público del pase: lo abre la visita desde el link que le comparte el residente, sin iniciar sesión.
    /// El token firmado del QR es la credencial: no se puede adivinar ni fabricar, y un pase cancelado responde 404.
    /// Solo expone lo que ya figura en el pase (sin teléfono ni email del residente).
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/passes")]
    public class PassesController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IPassDocumentService _passDocument;

        public PassesController(IAccessControlService accessControl, IPassDocumentService passDocument)
        {
            _accessControl = accessControl;
            _passDocument = passDocument;
        }

        [HttpGet("{token}")]
        public async Task<ActionResult<PassDto>> Get(string token)
            => Ok(await _accessControl.GetPublicPassAsync(token));

        [HttpGet("{token}/pdf")]
        public async Task<IActionResult> GetPdf(string token)
        {
            var pass = await _accessControl.GetPublicPassAsync(token);
            return File(_passDocument.GeneratePdf(pass), "application/pdf", EntryPermissionsController.PassFileName(pass));
        }
    }
}
