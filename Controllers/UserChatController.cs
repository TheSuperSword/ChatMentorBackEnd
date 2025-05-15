using System.Security.Claims;
using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.Core.Services.UserChatServices;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model.UserChat_Models;
using ChatMentor.Backend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatMentor.Backend.API.Controllers;

[ApiController]
[Route("api/userchat")]
public class UserChatController : ControllerBase
{
    private readonly UserConnectionService _userConnectionService;
    private readonly ConversationService _conversationService;
    private readonly MessageService _messageService;
    private readonly DocumentService _documentService;

    public UserChatController(UserConnectionService userConnectionService, ConversationService conversationService, MessageService messageService, DocumentService documentService)
    {
        _userConnectionService = userConnectionService;
        _conversationService = conversationService;
        _messageService = messageService;
        _documentService = documentService;
    }

    // Endpoint to get all user connections
    [Authorize]
    [HttpGet("connections")]
    public async Task<ActionResult<List<UserConnection>>> GetAllConnections()
    {
        var connections = await _userConnectionService.GetAllConnectionsAsync();
        return Ok(JSendResponse<object>.Success(connections, "Connections retrieved successfully"));
    }
    
    // Create a new Conversation
    [Authorize]
    [HttpPost("conversations")]
    public async Task<ActionResult<Conversation>> CreateConversation([FromBody] CreateConversationRequest conversation)
    {
        // Get the userId from the claims (this assumes you're using a standard JWT token with the user ID in claims)
        var userId = GetUserIdFromClaims();
        var createdConversation = await _conversationService.CreateConversationAsync(conversation, userId);
        if (createdConversation == null)
        {
            return BadRequest(JSendResponse<object>.Error("Failed to create conversation"));
        }
        return Ok(JSendResponse<object>.Success(createdConversation, "Conversation created successfully"));
    }
    
    // Get all conversations for a user
    [Authorize]
    [HttpGet("conversations")]
    public async Task<ActionResult<List<Conversation>>> GetConversations()
    {
        // Get the userId from the claims (this assumes you're using a standard JWT token with the user ID in claims)
        var userId = GetUserIdFromClaims();
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        return Ok(JSendResponse<object>.Success(conversations, "Conversations retrieved successfully"));
    }
    
    // Get a specific conversation by ID
    [Authorize]
    [HttpGet("conversations/{conversationId}")]
    public async Task<ActionResult<Conversation>> GetConversation(Guid conversationId)
    {
        // Get the userId from the claims (this assumes you're using a standard JWT token with the user ID in claims)
        var userId = GetUserIdFromClaims();
        var conversation = await _conversationService.GetConversationByIdAsync(conversationId, userId);
        if (conversation == null)
        {
            return NotFound(JSendResponse<object>.Fail(null,"Conversation not found"));
        }
        else
        {
            return Ok(JSendResponse<object>.Success(conversation, "Conversation retrieved successfully"));
        }
    }
    
    // Add a user to a conversation
    [Authorize]
    [HttpPost("conversations/{conversationId}/users")]
    public async Task<ActionResult<bool>> AddUserToConversation(Guid conversationId, [FromBody] Guid userId)
    {
        // Get the userId from the claims (this assumes you're using a standard JWT token with the user ID in claims)
        var currentUserId = GetUserIdFromClaims();
        var added = await _conversationService.AddUserToConversationAsync(conversationId, userId.ToString(), currentUserId);
        return Ok(JSendResponse<object>.Success(added, "User added to conversation successfully"));
    }
    
    // Remove a user from a conversation
    [Authorize]
    [HttpDelete("conversations/{conversationId}/users/{userId}")]
    public async Task<ActionResult<bool>> RemoveUserFromConversation(Guid conversationId, Guid userId)
    {
        var currentUserId = GetUserIdFromClaims();
        var removed = await _conversationService.RemoveUserFromConversationAsync(conversationId, userId.ToString(), currentUserId);
        return Ok(JSendResponse<object>.Success(removed, "User removed from conversation successfully"));
    }
    
    // Delete a conversation
    [Authorize]
    [HttpDelete("conversations/{conversationId}")]
    public async Task<ActionResult<bool>> DeleteConversation(Guid conversationId)
    {
        // Get the userId from the claims (this assumes you're using a standard JWT token with the user ID in claims)
        var userId = GetUserIdFromClaims();
        var deleted = await _conversationService.DeleteConversationAsync(conversationId, userId);
        return Ok(JSendResponse<object>.Success(deleted, "Conversation deleted successfully"));
    }
    
    // Send a message
    [Authorize]
    [HttpPost("messages")]
    public async Task<ActionResult<Message>> SendMessage([FromForm] Guid conversationId, [FromForm] string content, [FromForm] Guid? replyToMessageId, [FromForm] List<IFormFile>? attachments)
    {
        try
        {
            // Get the userId from the claims
            var userId = GetUserIdFromClaims();
            
            // Create message request with attachments
            var messageRequest = new MessageRequest
            {
                ConversationId = conversationId,
                Content = content,
                ReplyToMessageId = replyToMessageId,
                Attachments = attachments
            };
            
            var sentMessage = await _messageService.SendMessageAsync(messageRequest, userId);
            return Ok(JSendResponse<object>.Success(sentMessage, "Message sent successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(JSendResponse<object>.Error($"Failed to send message: {ex.Message}"));
        }
    }
    
    // Get messages by conversation ID
    [Authorize]
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<ActionResult<List<Message>>> GetMessages(Guid conversationId, [FromQuery] Guid? cursorMessageId = null, [FromQuery] int limit = 20)
    {
        // Get the userId from the claims (this assumes you're using a standard JWT token with the user ID in claims)
        var userId = GetUserIdFromClaims();
        var messages = await _messageService.GetConversationMessagesAsync(conversationId, userId, cursorMessageId, limit);
        
        var paginationMeta = new CursorPaginationMeta(
            currentPage: 1,
            pageSize: limit,
            totalRecords: messages.Count,
            nextCursor: messages.Count > 0 ? messages[^1].MessageId : null,
            hasMore: messages.Count == limit
        );
    
        return Ok(JSendResponse<object>.Success(messages, "Messages retrieved successfully", paginationMeta));
    }
    
    // Update a message
    [Authorize]
    [HttpPut("messages/{messageId}")]
    public async Task<ActionResult<bool>> UpdateMessage(Guid messageId, [FromBody] String newMessage)
    {
        // Get the userId from the claims (this assumes you're using a standard JWT token with the user ID in claims)
        var userId = GetUserIdFromClaims();
        var updated = await _messageService.EditMessageAsync(messageId, newMessage, userId);
        return Ok(JSendResponse<object>.Success(updated, "Message updated successfully"));
    }
    
    // Delete a message
    [Authorize]
    [HttpDelete("messages/{messageId}")]
    public async Task<ActionResult<bool>> DeleteMessage(Guid messageId)
    {
        // Get the userId from the claims (this assumes you're using a standard JWT token with the user ID in claims)
        var userId = GetUserIdFromClaims();
        var deleted = await _messageService.DeleteMessageAsync(messageId, userId);
        return Ok(JSendResponse<object>.Success(deleted, "Message deleted successfully"));
    }
    
    
    [Authorize]
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadDocument(string id)
    {
        try
        {
            var (filePath, fileName, contentType) = await _documentService.PrepareFileForDownloadAsync(id);
        
            // For security, use PhysicalFileResult to prevent path traversal attacks
            return PhysicalFile(filePath, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while downloading the document.");
        }
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