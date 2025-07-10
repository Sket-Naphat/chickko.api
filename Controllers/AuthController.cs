using Microsoft.AspNetCore.Mvc;
using chickko.api.Services;
using chickko.api.Models;
using Microsoft.AspNetCore.Authorization;

namespace chickko.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var loginUser = _authService.Login(request.Username, request.Password);
            if (loginUser == null)
                return Unauthorized("Invalid username or password");

            var token = _authService.GenerateJwtToken(loginUser);
            return Ok(new { token });
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _authService.Register(request);
            if (user == null)
                return BadRequest("User registration failed");

            return CreatedAtAction(nameof(Login), new { username = user.Username }, user);
        }
    }
}