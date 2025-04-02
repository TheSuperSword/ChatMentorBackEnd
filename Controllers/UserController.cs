using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatMentor.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(JSendResponse<string>.Fail(null, "User not found"));
            }
            return Ok(JSendResponse<object>.Success(user, "User retrieved successfully"));
        }
        
        [Authorize]
        [HttpGet("guid/{guid}")]
        public async Task<IActionResult> GetUserByGuid(string guid)
        {
            var user = await _userService.GetUserByGuidAsync(guid);
            if (user == null)
            {
                return NotFound(JSendResponse<string>.Fail(null, "User not found"));
            }
            return Ok(JSendResponse<object>.Success(user, "User retrieved successfully"));
        }
        
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Paginated user retrieval
                var (users, paginationMeta) = await _userService.GetPaginatedUsersAsync(page, pageSize);

                if (!users.Any())
                {
                    return NotFound(JSendResponse<string>.Fail(null, "No users found"));
                }

                // Respond with paginated data and metadata
                return Ok(JSendResponse<object>.Success(users, "Users retrieved successfully", paginationMeta));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(JSendResponse<string>.Fail(null, ex.Message));
            }
        }
        
    }
}