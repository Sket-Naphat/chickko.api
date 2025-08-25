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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // site อ่านผ่าน Header โดย SiteService แล้ว ไม่ต้องรับจาก body
            var result = await _authService.LoginAsync(request.Username, request.Password);
            return Ok(result);
        }

        // [HttpPost("register")]
        // [AllowAnonymous]
        // public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        // {
        //     try
        //     {
        //         var user = await _authService.Register(request);
        //         return Ok(user);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }
    }
}