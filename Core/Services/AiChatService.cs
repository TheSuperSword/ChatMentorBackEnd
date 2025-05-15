using System.Security.Claims;
using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model;
using OpenAI.Chat;

namespace ChatMentor.Backend.Core.Services;

public class AiChatService
{
    private readonly IAiChatRepository _aiChatRepository;
    private readonly IExternalAiRepository _externalAiRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly UserService _userService;

    public AiChatService(
        IExternalAiRepository externalAiRepository,
        IAiChatRepository aiChatRepository,
        IAiChatSessionRepository sessionRepository,
        IHttpContextAccessor httpContextAccessor, UserService userService)
    {
        _externalAiRepository = externalAiRepository;
        _aiChatRepository = aiChatRepository;
        _sessionRepository = sessionRepository;
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public async Task<AiChatSessionResponseDTO> CreateNewSessionAsync(string initialTitle = "New Chat")
    {
        // 🛡 Get User ID from Claims
        var userId = GetUserIdFromClaims();
        var session = new AiChatSession
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            Title = initialTitle,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
        var response = await _sessionRepository.CreateSessionAsync(session);
        var sessionDto = new AiChatSessionResponseDTO
        {
            SessionId = response.SessionId,
            UserId = response.UserId,
            Title = response.Title,
            SessionStatus = response.SessionStatus
        };

        return sessionDto;
    }

    public async Task<AIChatDto> SendMessageAsync(Guid sessionId, string userMessage)
    {
        // Validate session existence + update last updated timestamp
        var sessionUpdated = await _sessionRepository.UpdateLastUpdatedAtAsync(sessionId);
        if (!sessionUpdated) throw new KeyNotFoundException("Session not found.");

        // 🛡 Get User Id from Claims and Get User Details
        var userId = GetUserIdFromClaims();
        var userDetails = await _userService.GetUserByGuidAsync(userId.ToString());

        // 🛡 Optional: Verify session actually belongs to the user
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to access this session.");

        // 🧠 Get a user's background (tags, headline, etc.) for AI personalization
        var userBackgroundPrompt = GenerateUserBackgroundPrompt(userDetails);

        // 🗨 Get chat history for the session
        var chatHistory = await GetChatHistoryForAiAsync(sessionId);

        // 💬 Inject a user background into AI prompt
        chatHistory.Insert(0, new SystemChatMessage(userBackgroundPrompt));

        // 🔍 Add the current user message to the chat history
        chatHistory.Add(new UserChatMessage(userMessage));

        // 🧠 Send everything to external AI to get the response
        var aiResponse = await _externalAiRepository.GetAiResponseWithChatHistoryAsync(chatHistory);

        // 💾 Save the user message + AI response
        var userChat = new AIChat
        {
            SessionId = sessionId,
            UserMessage = userMessage,
            AIResponse = aiResponse,
            Timestamp = DateTime.UtcNow
        };

        await _aiChatRepository.CreateChatAsync(userChat);

        // 🏷 If it's the first message(s), generate a session title
        var chatsInSession = await _aiChatRepository.GetChatsBySessionIdAsync(sessionId, 1, 3);
        if (chatsInSession.Count <= 2)
        {
            var sessionTitle = GenerateSessionTitle(userMessage);
            await _sessionRepository.UpdateSessionTitleAsync(sessionId, sessionTitle);
        }

        // 📨 Return final result DTO
        return new AIChatDto
        {
            SessionId = sessionId,
            UserMessage = userMessage,
            AIResponse = aiResponse,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<List<AIChatDto>> GetChatHistoryAsync(Guid sessionId, int pageNumber = 1, int pageSize = 50)
    {
        var userId = GetUserIdFromClaims();
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to access this chat.");
        var chatHistory = await _aiChatRepository.GetChatsBySessionIdAsync(sessionId, pageNumber, pageSize);
        return chatHistory.Select(chat => new AIChatDto
        {
            SessionId = chat.SessionId, UserMessage = chat.UserMessage, AIResponse = chat.AIResponse,
            Timestamp = chat.Timestamp
        }).ToList();
    }

    public async Task<List<AiChatSessionResponseDTO>> GetUserSessionsAsync(int pageNumber = 1, int pageSize = 20)
    {
        var userId = GetUserIdFromClaims();
        var sessionsList = await _sessionRepository.GetSessionsByUserIdAsync(userId, pageNumber, pageSize);
        return sessionsList.Select(session => new AiChatSessionResponseDTO
        {
            SessionId = session.SessionId, UserId = session.UserId, Title = session.Title,
            SessionStatus = session.SessionStatus
        }).ToList();
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        var userId = GetUserIdFromClaims();
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to access this chat.");
        return await _sessionRepository.DeleteSessionAsync(sessionId);
    }


    // Helper methods
    private string GenerateSessionTitle(string userMessage)
    {
        // Simple logic to generate a title from the first user message
        var title = userMessage.Length <= 30
            ? userMessage
            : userMessage.Substring(0, 27) + "...";

        return title;
    }

    private string GenerateUserBackgroundPrompt(UserDto? user)
    {
        if (user == null)
            return "You are a university student or recent graduate seeking academic and career guidance.";

        string CleanText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var cleaned = input.Trim();
            var meaninglessInputs = new[] { "string", "test", "abc", "default", "-", "n/a", "none" };

            if (meaninglessInputs.Contains(cleaned.ToLower()) || cleaned.Length < 5)
                return string.Empty;

            return cleaned;
        }

        var headline = CleanText(user.Headline);
        var bio = CleanText(user.Bio);

        var headlinePart = !string.IsNullOrEmpty(headline)
            ? $"Their headline is: '{headline}'."
            : "";

        var tagsPart = user.Tags.Any()
            ? $"Your fields of interest include: {string.Join(", ", user.Tags)}."
            : "";

        var bioPart = !string.IsNullOrEmpty(bio)
            ? $"Their bio says: {bio}."
            : "";

        return
            $"The user is a university student or recent graduate. {headlinePart} {tagsPart} {bioPart} Always provide academic, skill development, and career advice that is highly relevant.";
    }

    private async Task<List<ChatMessage>> GetChatHistoryForAiAsync(Guid sessionId)
    {
        // Start with a system prompt
        var systemPrompt =
            "You are an academic and career mentor specializing in providing helpful, accurate, and supportive advice to university students and fresh graduates across a wide range of fields of study. " +
            "Always tailor your responses based on the user's question. " +
            "When answering academic questions, explain concepts in a clear, detailed, and easy-to-understand way, appropriate to the user's level. " +
            "When providing career guidance, offer practical, realistic, and motivational advice, suggesting actionable steps. " +
            "If the question is unclear, ask for clarification, but do not provide generic or motivational answers unless explicitly requested. " + // Ensures specificity
            "If the question involves personal opinions, important decisions, or fields outside your expertise, offer general guidance and encourage consulting a real mentor or professional.\n" +
            "Always maintain a positive, respectful, and professional tone. " +
            "Ensure responses are relevant to the user's specific query, avoiding generic advice that does not directly address the question.";

        var chatMessages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt)
        };

        // Approximate token count for system prompt
        var totalTokens = EstimateTokenCount(systemPrompt);
        var maxTokens = 4000; // Adjust based on your model's context window

        // Get all messages in this session (get more than needed to filter)
        var messages = await _aiChatRepository.GetChatsBySessionIdAsync(sessionId, 1, 50);

        // Process messages in reverse chronological order (newest first)
        var orderedMessages = messages.OrderByDescending(m => m.Timestamp).ToList();

        // Start with the most recent message and work backwards until we hit the token limit
        var includedMessages = new List<AIChat>();

        foreach (var msg in orderedMessages)
        {
            var msgTokens = EstimateTokenCount(msg.UserMessage) + EstimateTokenCount(msg.AIResponse);

            // Check if adding this message would exceed our token limit
            if (totalTokens + msgTokens > maxTokens)
                break;

            totalTokens += msgTokens;
            includedMessages.Add(msg);
        }

        // Reverse back to chronological order for the conversation flow
        includedMessages.Reverse();

        // Convert to ChatMessage format
        foreach (var msg in includedMessages)
        {
            chatMessages.Add(new UserChatMessage(msg.UserMessage));
            chatMessages.Add(new AssistantChatMessage(msg.AIResponse));
        }

        return chatMessages;
    }

    private int EstimateTokenCount(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        // GPT models typically use ~4 characters per token on average for English
        // This is a rough estimate - for production use a proper tokenizer
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    private Guid GetUserIdFromClaims()
    {
        // 🛡 Get User Id from Claims
        var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) throw new UnauthorizedAccessException("User ID not found in token.");
        if (!Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("Invalid User ID format.");
        return userId;
    }
}