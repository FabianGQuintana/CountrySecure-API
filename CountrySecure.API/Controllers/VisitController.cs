using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;
        private readonly ICurrentUserService _currentUser;

        public VisitController(IVisitService visitService, ICurrentUserService currentUser)
        {
            _visitService = visitService;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<IActionResult> AddNewVisitAsync([FromBody] CreateVisitDto newVisitDto)
        {
            var createdVisit = await _visitService.AddNewVisitAsync(newVisitDto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetVisitById), new { visitId = createdVisit.VisitId }, createdVisit);
        }

        [HttpGet("{visitId:guid}")]
        public async Task<IActionResult> GetVisitById(Guid visitId)
        {
            var visit = await _visitService.GetVisitByIdAsync(visitId)
                        ?? throw new KeyNotFoundException("Visit not found");
            return Ok(visit);
        }

        [HttpGet("dni/{dniVisit:int}")]
        public async Task<IActionResult> GetVisitsByDni(int dniVisit)
            => Ok(await _visitService.GetVisitsByDniAsync(dniVisit));

        [HttpGet]
        public async Task<IActionResult> GetAllVisits([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => Ok(await _visitService.GetAllVisitsAsync(pageNumber, pageSize));

        // Visita completa junto con todos sus permisos asociados.
        [HttpGet("{visitId:guid}/with-permits")]
        public async Task<IActionResult> GetVisitWithPermits(Guid visitId)
        {
            var visit = await _visitService.GetVisitWithPermitsAsync(visitId)
                        ?? throw new KeyNotFoundException("Visit not found");
            return Ok(visit);
        }

        // Solo los permisos válidos (Pending) de la visita.
        [HttpGet("{visitId:guid}/permits/valid")]
        public async Task<IActionResult> GetValidPermits(Guid visitId)
            => Ok(await _visitService.GetValidPermitsByVisitIdAsync(visitId));

        [HttpPut("{visitId:guid}")]
        public async Task<IActionResult> UpdateVisit(Guid visitId, [FromBody] UpdateVisitDto updateVisitDto)
            => Ok(await _visitService.UpdateVisitAsync(visitId, updateVisitDto));

        [HttpPatch("{visitId:guid}/SoftDelete")]
        public async Task<IActionResult> SoftDeleteToggle(Guid visitId)
        {
            var updatedVisitDto = await _visitService.SoftDeleteToggleAsync(visitId, _currentUser.GetRequiredUserId())
                                  ?? throw new KeyNotFoundException($"Visit with ID {visitId} not found.");

            var action = updatedVisitDto.VisitStatus == "Active" ? "reactivated" : "deactivated";
            return Ok(new
            {
                Message = $"The Visit with ID {visitId} has been {action} successfully.",
                Visit = updatedVisitDto
            });
        }

        // Incluye visitas dadas de baja: solo Admin.
        [HttpGet("all")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetAllVisitsWithoutFilter()
            => Ok(await _visitService.GetAllVisitsWithoutFilterAsync());
    }
}
