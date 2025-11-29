using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using CountrySecure.Application.DTOs.Properties;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CountrySecure.API.Controllers
{
    // Atributos de la API
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        // 1. Método: GET /api/Property?pageNumber=1&pageSize=10
        // Ahora devuelve IEnumerable<PropertyDto>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // El servicio devuelve DTOs
            var propertiesDto = await _propertyService.GetAllPropertiesAsync(pageNumber, pageSize);

            return Ok(propertiesDto); // 200 OK
        }

        // 2. Método: GET /api/Property/available?pageNumber=1&pageSize=10
        // Ahora devuelve IEnumerable<PropertyDto>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Llama al servicio con el Enum de Dominio
            var availablePropertiesDto = await _propertyService.GetPropertiesByStatusAsync(
                Domain.Enums.PropertyStatus.Available,
                pageNumber,
                pageSize
            );
            return Ok(availablePropertiesDto); // 200 OK
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var propertyDto = await _propertyService.GetPropertyByIdAsync(id);

            if (propertyDto == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(propertyDto); // 200 OK
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreatePropertyDto dto)
        {
            // Valida los Data Annotations del DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            // El servicio mapea el DTO a Entidad, guarda, mapea de vuelta a DTO, y lo devuelve.
            var propertyDto = await _propertyService.AddNewPropertyAsync(dto);

            // 201 Created
            return CreatedAtAction(nameof(GetById), new { id = propertyDto.IdProperty }, propertyDto);
        }

        // 5. Método: DELETE /api/Property/{id} (Baja Lógica)
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            bool deleted = await _propertyService.SoftDeletePropertyAsync(id);

            if (!deleted)
            {
                return NotFound(); // 404 Not Found
            }
            return NoContent(); // 204 No Content
        }

     
    }
}