using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.DTOs;

namespace ChatMentor.Backend.Core.Services;

public class UserStatsService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAiChatSessionRepository _sessionRepository;
    private readonly IKnowledgeBaseRepository _knowledgeBaseRepository;


    public UserStatsService(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IAiChatSessionRepository sessionRepository,
        IKnowledgeBaseRepository knowledgeBaseRepository)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _knowledgeBaseRepository = knowledgeBaseRepository;
    }

    public async Task<int> GetTotalConversationsAsync(Guid userGuid)
    {
        var user = await _userRepository.GetUserByGuidAsync(userGuid.ToString());
        if (user == null) throw new KeyNotFoundException("User not found");
        var conversations = await _conversationRepository.GetUserConversationAsync(user.Id);
        return conversations?.Count ?? 0;
    }

    public async Task<int> GetTotalAiSessionAsync(Guid userGuid)
    {
        var user = await _userRepository.GetUserByGuidAsync(userGuid.ToString());
        if (user == null) throw new KeyNotFoundException("User not found");
        var sessionsList = await _sessionRepository.GetSessionsByUserIdAsync(userGuid, 1, 50);
        return sessionsList?.Count ?? 0;
    }

    public async Task<int> GetTotalKnowledgeBaseAsync()
    {
        var kbList = await _knowledgeBaseRepository.GetAllKnowledgeBasesAsync();
        return kbList?.Count() ?? 0;
    }

    public async Task<UserStatDTOs> GetUserStatsAsync(Guid userGuid)
    {
        var userStats = new UserStatDTOs
        {
            totalAiChatSessions = await GetTotalAiSessionAsync(userGuid),
            totalKnowledgeItems = await GetTotalKnowledgeBaseAsync(),
            totalMentorSessions = await GetTotalConversationsAsync(userGuid),
        };
        return userStats;
    }
}