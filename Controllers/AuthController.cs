using Microsoft.AspNetCore.Mvc;
using chickko.api.Services;
using chickko.api.Models;
using Microsoft.AspNetCore.Authorization;
using chickko.api.Interface;

namespace chickko.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ISiteService _siteService;

        public AuthController(IAuthService authService, ISiteService siteService)
        {
            _authService = authService;
            _siteService = siteService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // site อ่านผ่าน Header โดย SiteService แล้ว ไม่ต้องรับจาก body
            var result = await _authService.LoginAsync(request.Username, request.Password);
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var currentSite = _siteService.GetCurrentSite();
                var result = await _authService.Register(request);
                
                if (result == false)
                {
                    return BadRequest(new { 
                        success = false,
                        message = "Registration failed.",
                        site = currentSite,
                        timestamp = DateTime.UtcNow
                    });
                }
                
                return Ok(new { 
                    success = true,
                    message = "User registered successfully",
                    site = currentSite,
                    username = request.Username,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false,
                    message = ex.Message,
                    site = _siteService.GetCurrentSite(),
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}