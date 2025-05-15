using Azure;
using ChatMentor.Backend.Core.Services;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using Azure.Core;
using Azure.AI.OpenAI;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Responses;
using Microsoft.AspNetCore.Authorization;

namespace ChatMentor.Backend.Controllers;

[ApiController]
[Route("api/aichat")]
public class AiChatController : ControllerBase
{
    private readonly AiChatService _aiChatService;

    public AiChatController(AiChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }
    [Authorize]
    [HttpPost("start")]
    public async Task<IActionResult> StartNewSession()
    {
        var session = await _aiChatService.CreateNewSessionAsync();
        return Ok(JSendResponse<object>.Success(session, "New session started successfully."));
    }
    
    [Authorize]
    [HttpPost("send/{sessionId}")]
    public async Task<IActionResult> SendMessage([FromRoute] Guid sessionId, [FromBody] SendMessageRequest request)
    {
        if (sessionId == Guid.Empty)
        {
            return BadRequest(JSendResponse<object>.Error("Session ID cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(request?.Message))
        {
            return BadRequest(JSendResponse<object>.Error("User message cannot be empty."));
        }

        var chat = await _aiChatService.SendMessageAsync(sessionId, request.Message);
        return Ok(JSendResponse<object>.Success(chat, "Message sent successfully."));
    }

    
    [Authorize]
    [HttpGet("history/{sessionId}")]
    public async Task<IActionResult> GetChatHistory([FromRoute] Guid sessionId, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (sessionId == Guid.Empty)
        {
            return BadRequest(JSendResponse<object>.Error("Session ID cannot be empty."));
        }
        var chatHistory = await _aiChatService.GetChatHistoryAsync(sessionId, page, size);
        return Ok(JSendResponse<object>.Success(chatHistory, "Chat history retrieved successfully."));
    }
    
    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetUserSessions([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var sessions = await _aiChatService.GetUserSessionsAsync(page, size);
        return Ok(JSendResponse<object>.Success(sessions, "User sessions retrieved successfully."));
    }
    
    [Authorize]
    [HttpDelete("delete/{sessionId}")]
    public async Task<IActionResult> DeleteSession([FromRoute] Guid sessionId)
    {
        if (sessionId == Guid.Empty)
        {
            return BadRequest(JSendResponse<object>.Error("Session ID cannot be empty."));
        }
        var result = await _aiChatService.DeleteSessionAsync(sessionId);
        if (result)
        {
            return Ok(JSendResponse<object>.Success(null, "Session deleted successfully."));
        }
        return NotFound(JSendResponse<object>.Error("Session not found."));
    }
}