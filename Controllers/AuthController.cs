using Microsoft.AspNetCore.Mvc;
using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Responses; // Assuming this is where your services are

namespace ChatMentor.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Register Method
        [HttpPost("register")]
        [Consumes("multipart/form-data")] // This explicitly tells Swagger to use multipart/form-data
        public async Task<IActionResult> Register([FromForm] RegisterUserDto registerDto, IFormFile? imageFile)
        {
            var userDto = await _authService.RegisterUserAsync(registerDto, imageFile);
            if (userDto == null)
            {
                // Return JSend Fail response
                return BadRequest(JSendResponse<object>.Fail(null, "Registration failed."));
            }

            // Return JSend Success response
            return CreatedAtAction(
                nameof(Register),
                new { email = userDto.Email },
                JSendResponse<object>.Success(userDto)
            );
        }

        // Login Method
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
        {
            var token = await _authService.LoginUserAsync(loginDto);
            if (token == null)
            {
                // Return JSend Fail response
                return Unauthorized(JSendResponse<object>.Fail(null, "Login failed."));
            }

            // Return JSend Success response
            return Ok(JSendResponse<object>.Success(token, "Login successful."));

        }
    }
}