using ChatMentor.Backend.Model;

namespace ChatMentor.Backend.DTOs;

// Create Session Response DTO
public class AiChatSessionResponseDTO
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string? Title { get; set; } = string.Empty;
    public SessionStatus SessionStatus { get; set; }
}

// Create/Send Chat Response DTO
public class AIChatDto
{
    public Guid SessionId { get; set; }
    public string UserMessage { get; set; }
    public string AIResponse { get; set; }
    public DateTime Timestamp { get; set; }
}

public class SendMessageRequest
{
    public string? Message { get; set; }
}
