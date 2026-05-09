using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Application.DTOs;
using OrderSystem.Infrastructure.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrderSystem.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public UsersController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _identityService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _identityService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            var createdBy = User.Identity?.Name ?? "Admin";
            var (success, error) = await _identityService.RegisterAsync(dto.Username, dto.Password, dto.Role, createdBy);
            if (!success) return BadRequest(new { message = error });
            return Ok(new { message = "User created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UserUpdateDto dto)
        {
            var (success, error) = await _identityService.UpdateUserAsync(id, dto);
            if (!success) return BadRequest(new { message = error });
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _identityService.DeleteUserAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "User deleted successfully" });
        }
    }
}
