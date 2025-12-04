using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {

        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? role = null)
        {
            var users = await _userService.GetAllAsync(page, size, role);
            return Ok(users);
        }

        [HttpGet("{id}")]      
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        // [HttpPost]
        // public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        // {
        //     var createdUser = await _userService.CreateUserAsync(dto);
        //     return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser); 
        // }

        [HttpPut("{id}")] 
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(id, dto);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }


        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var updatedUser = await _userService.ToggleActiveAsync(id);

            if (updatedUser == null)
            {
                return NotFound(new {message = $"User with id {id} not found"});
            }

            return Ok(updatedUser);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _userService.DeleteUserAsync(id);

            if (!deleted) return NotFound($"User with id: {id} not found");

            return NoContent();
        }

    }
}