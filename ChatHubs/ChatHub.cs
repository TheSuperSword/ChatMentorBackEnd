using System.Security.Claims;
using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.Core.Services;
using ChatMentor.Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatMentor.Backend.ChatHubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserConnectionService _userConnectionService;
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserRepository _userRepository;

        // Inject the services into the constructor
        public ChatHub(
            UserConnectionService userConnectionService, 
            IConversationRepository conversationRepository,
            IUserRepository userRepository)
        {
            _userConnectionService = userConnectionService;
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
        }
        
        public override async Task OnConnectedAsync()
        {
            await AddUserConnectionAsync();
            
            // Join user to all their conversation groups
            await JoinUserConversationGroups();
            
            await base.OnConnectedAsync();
        }
        
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await RemoveUserConnectionAsync();
            await base.OnDisconnectedAsync(exception);
        }

        // Ping Method
        public async Task Ping()
        {
            
        }
        
        // TypingIndicator method (Maybe will use)
        public async Task SendTypingIndicator(Guid conversationId)
        {
            var userId = GetUserIdFromClaims();
            
            // Check if user is part of the conversation
            if (await IsUserInConversation(conversationId, userId))
            {
                var user = await _userRepository.GetUserByGuidAsync(userId);
                var typingInfo = new
                {
                    UserId = userId,
                    UserName = $"{user.LastName} {user.LastName}",
                    ConversationId = conversationId
                };
                
                // Send typing indicator to the conversation group
                await Clients.Group($"conversation-{conversationId}")
                    .SendAsync("UserTyping", typingInfo);
            }
        }

        // Method to mark messages as read (Not used?)
        public async Task MarkMessageAsRead(Guid messageId, Guid conversationId)
        {
            var userId = GetUserIdFromClaims();
            
            // Check if user is part of the conversation
            if (await IsUserInConversation(conversationId, userId))
            {
                var readStatus = new 
                {
                    MessageId = messageId,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                };
                
                // Notify others that message was read
                await Clients.Group($"conversation-{conversationId}")
                    .SendAsync("MessageRead", readStatus);
            }
        }

        // Join specific conversation group (Not used?)
        public async Task JoinConversation(Guid conversationId)
        {
            var userId = GetUserIdFromClaims();
            
            // Check if user is member of conversation
            if (await IsUserInConversation(conversationId, userId))
            {
                // Add user to the SignalR group for this conversation
                await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
                
                // Notify other users in the group
                await Clients.Group($"conversation-{conversationId}")
                    .SendAsync("UserJoinedConversation", new { UserId = userId, ConversationId = conversationId });
            }
        }

        // Leave specific conversation group (Not used?)
        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
            
            var userId = GetUserIdFromClaims();
            
            // Notify other users in the group
            await Clients.Group($"conversation-{conversationId}")
                .SendAsync("UserLeftConversation", new { UserId = userId, ConversationId = conversationId });
        }
        
        // Helper method to join all user's conversation groups
        private async Task JoinUserConversationGroups()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var user = await _userRepository.GetUserByGuidAsync(userId);
                
                if (user != null)
                {
                    // Get all user's conversations
                    var conversations = await _conversationRepository.GetUserConversationAsync(user.Id);
                    
                    // Join each conversation group
                    foreach (var conversation in conversations)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversation.ConversationId}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error joining conversation groups: {ex.Message}");
            }
        }
        
        // This method is called when a user connects to the hub
        public async Task AddUserConnectionAsync()
        {
            // Get the userId from the claims
            var userId = GetUserIdFromClaims();

            var connection = new UserConnectionRequest
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                DeviceInfo = Context.GetHttpContext()?.Request.Headers.UserAgent.ToString(),
                IpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString()
            };
            
            // Call the service to add the connection
            await _userConnectionService.AddUserConnectionAsync(connection);

            // Notify other clients about this connection
            await Clients.Others.SendAsync("UserOnline", userId);
        }

        // This method is called when a user disconnects
        public async Task RemoveUserConnectionAsync()
        {
            // Get the userId from the claims
            var userId = GetUserIdFromClaims();
        
            // Call the service to remove the connection
            await _userConnectionService.RemoveUserConnectionAsync(Context.ConnectionId);
        
            // Check if user has any other active connections
            var connections = await _userConnectionService.GetAllConnectionsAsync();
            var userStillHasConnections = connections.Any(c => c.UserId == userId);
            
            // If user has no more connections, notify others they went offline
            if (!userStillHasConnections)
            {
                await Clients.Others.SendAsync("UserOffline", userId);
            }
        }
        
        // Helper method to check if user is in conversation
        private async Task<bool> IsUserInConversation(Guid conversationId, string userGuidString)
        {
            if (Guid.TryParse(userGuidString, out var userGuid))
            {
                var user = await _userRepository.GetUserByGuidAsync(userGuidString);
                if (user != null)
                {
                    return await _conversationRepository.IsUserInConversationAsync(conversationId, user.Id);
                }
            }
            return false;
        }

        // Helper method to extract userId from claims
        private string GetUserIdFromClaims()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new HubException("User not authenticated or invalid user ID");
            }
            return userIdClaim;
        }
    }
}