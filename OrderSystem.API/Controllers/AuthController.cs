using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Infrastructure.Services;

namespace OrderSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var (success, token, refreshToken, role, error) = await _identityService.LoginAsync(model.Username, model.Password);
            if (!success) return BadRequest(new { error });

            return Ok(new { token, refreshToken, role, username = model.Username });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestModel model)
        {
            var (success, token, refreshToken, error) = await _identityService.RefreshTokenAsync(model.Token, model.RefreshToken);
            if (!success) return BadRequest(new { error });

            return Ok(new { token, refreshToken });
        }
    }

    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshRequestModel
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
