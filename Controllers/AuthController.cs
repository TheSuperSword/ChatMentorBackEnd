using System.Security.Claims;
using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Assuming this is where your services are

namespace ChatMentor.Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // Register Method
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
    {
        var userDto = await _authService.RegisterUserAsync(registerDto);
        if (userDto == null)
            // Return JSend Fail response
            return BadRequest(JSendResponse<object>.Fail(null, "Registration failed."));

        // Return JSend Success response
        return CreatedAtAction(
            nameof(Register),
            new { email = userDto.Email },
            JSendResponse<object>.Success(userDto, "Registration successful.")
        );
    }

    // Login Method
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
    {
        var token = await _authService.LoginUserAsync(loginDto);
        if (token == null)
            // Return JSend Fail response
            return Unauthorized(JSendResponse<object>.Fail(null, "Login failed."));

        // Return JSend Success response
        return Ok(JSendResponse<object>.Success(token, "Login successful."));
    }

    // Add new endpoint for refreshing tokens
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(JSendResponse<object>.Fail(null, "Invalid client request."));

        var tokenResponse = await _authService.RefreshTokenAsync(request);
        if (tokenResponse == null)
            return Unauthorized(JSendResponse<object>.Fail(null, "Invalid token or refresh token expired."));

        return Ok(JSendResponse<object>.Success(tokenResponse, "Token refreshed successfully."));
    }

    // Add revoke endpoint (logout)
    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized(JSendResponse<object>.Fail(null, "Invalid token."));

        var result = await _authService.RevokeRefreshTokenAsync(userId);
        if (!result) return BadRequest(JSendResponse<object>.Fail(null, "Failed to revoke token."));

        return Ok(JSendResponse<object>.Success(null, "Token revoked successfully."));
    }
}