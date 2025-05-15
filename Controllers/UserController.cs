using System.Security.Claims;
using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatMentor.Backend.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly UserStatsService _userStatsService;

    public UserController(UserService userService, UserStatsService userStatsService)
    {
        _userService = userService;
        _userStatsService = userStatsService;
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound(JSendResponse<string>.Fail(null, "User not found"));
        return Ok(JSendResponse<object>.Success(user, "User retrieved successfully"));
    }

    [Authorize]
    [HttpGet("guid/{guid}")]
    public async Task<IActionResult> GetUserByGuid(string guid)
    {
        var user = await _userService.GetUserByGuidAsync(guid);
        if (user == null) return NotFound(JSendResponse<string>.Fail(null, "User not found"));
        return Ok(JSendResponse<object>.Success(user, "User retrieved successfully"));
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        try
        {
            // Paginated user retrieval
            var (users, paginationMeta) = await _userService.GetPaginatedUsersAsync(page, pageSize);

            if (!users.Any()) return NotFound(JSendResponse<string>.Fail(null, "No users found"));

            // Respond with paginated data and metadata
            return Ok(JSendResponse<object>.Success(users, "Users retrieved successfully", paginationMeta));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(JSendResponse<string>.Fail(null, ex.Message));
        }
    }

    [Authorize]
    [HttpGet ("userstats")]
    public async Task<IActionResult> GetUsersStats()
    {
        var userGuid = GetUserIdFromClaims();
        var userStats = await _userStatsService.GetUserStatsAsync(userGuid);
        return Ok(JSendResponse<object>.Success(userStats, "User Stats retrieved successfully"));
    }

    [Authorize]
    [HttpPut()]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
    {
        var userGuid = GetUserIdFromClaims();
        var updatedUser = await _userService.UpdateUserProfileAsync(userGuid.ToString(), updateUserDto);
        if(updatedUser != null) return Ok(JSendResponse<object>.Success(updatedUser, "Successfully updated user profile"));
        return BadRequest(JSendResponse<object>.Fail(null, "Failed to update user profile"));
    }
    
    
    // Helper method to extract userId from claims
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User not authenticated or invalid user ID");
        }
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID format");
        }
        return userId;
    }
}