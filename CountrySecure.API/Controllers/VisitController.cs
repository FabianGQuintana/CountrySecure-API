using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using CountrySecure.Application.DTOs.Visits;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Collections.Generic;


namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitController(IVisitService visitService)
        {
            _visitService = visitService;
        }


        [HttpPost]
        public async Task<IActionResult> AddNewVisitAsync([FromBody] CreateVisitDto newVisitDto)
        {
            var userId = GetCurrentUserId();

            var createdVisit = await _visitService.AddNewVisitAsync(newVisitDto, userId);

            return CreatedAtAction(nameof(GetVisitById),
                new { visitId = createdVisit.VisitId },
                createdVisit);
        }
        
        //  GET BY ID
        [HttpGet("{visitId:guid}")]
        public async Task<IActionResult> GetVisitById(Guid visitId)
        {
            var visit = await _visitService.GetVisitByIdAsync(visitId);

            if (visit == null) return NotFound("Visit not found");

            return Ok(visit);
        }

        //  GET BY DNI
        [HttpGet("dni/{dniVisit:int}")]
        public async Task<IActionResult> GetVisitsByDni(int dniVisit)
        {
            var visits = await _visitService.GetVisitsByDniAsync(dniVisit);
            return Ok(visits);
        }

        //  GET ALL (PAGINATED)
        [HttpGet]
        public async Task<IActionResult> GetAllVisits([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var visits = await _visitService.GetAllVisitsAsync(pageNumber, pageSize);
            return Ok(visits);
        }

        // // GET VISIT + PERMITS
        //// Obtiene la visita completa(sus datos) junto con todos sus permisos asociados.

        [HttpGet("{visitId:guid}/with-permits")]
           public async Task<IActionResult> GetVisitWithPermits(Guid visitId)
          {
              var visit = await _visitService.GetVisitWithPermitsAsync(visitId);

              if (visit == null) return NotFound("Visit not found");

              return Ok(visit);
          }


        //  GET VALID PERMIT
        // Devuelve todos los permisos validos asociados a esa visita, solo los permisos, sin los datos de la visita.

        [HttpGet("{visitId:guid}/permits/valid")]
        public async Task<IActionResult> GetValidPermits(Guid visitId)
        {
            var permits = await _visitService.GetValidPermitsByVisitIdAsync(visitId);

            

            return Ok(permits);
        }

        

        //  UPDATE
        [HttpPut("{visitId:guid}")]
        public async Task<IActionResult> UpdateVisit(Guid visitId, [FromBody] UpdateVisitDto updateVisitDto)
        {
            
            var updatedVisit = await _visitService.UpdateVisitAsync(visitId, updateVisitDto);
            return Ok(updatedVisit);

        }

        [HttpPatch("{visitId:guid}/SoftDelete")] 
        public async Task<IActionResult> SoftDeleteToggle(Guid visitId)
        {
            try
            {

                var currentUserId = GetCurrentUserId();

              
                var updatedVisitDto = await _visitService.SoftDeleteToggleAsync(visitId, currentUserId);

                if (updatedVisitDto == null)
                {
                    return NotFound(new { message = $"Visit with ID {visitId} not found." });
                }

                var action = updatedVisitDto.VisitStatus == "Active" ? "reactivated" : "deactivated";

                return Ok(new
                {
                    Message = $"The Visit with ID {visitId} has been {action} successfully.",
                    Visit = updatedVisitDto
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error while changing visit status.", detail = ex.Message });
            }
        }

        // VIEW ALL
        [HttpGet("all")]
        public async Task<IActionResult> GetAllVisitsWithoutFilter()
        {
            var visits = await _visitService.GetAllVisitsWithoutFilterAsync();
            return Ok(visits);
        }

        //  UTILITY: Obtener UserId autenticado
        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return Guid.Parse(userId);
        }
    }
}
