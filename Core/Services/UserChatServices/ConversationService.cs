using ChatMentor.Backend.ChatHubs;
using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model.UserChat_Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatMentor.Backend.Core.Services.UserChatServices;

public class ConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<ChatHub> _chatHubContext;

    public ConversationService(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IHubContext<ChatHub> chatHubContext)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _chatHubContext = chatHubContext;
    }

    public async Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request,
        Guid currentUserGuid)
    {
        // Convert all GUIDs to user IDs
        var userIds = new List<int>();
        foreach (var guidString in request.ParticipantUserGuids)
        {
            if (!Guid.TryParse(guidString, out var userGuid))
                throw new ArgumentException($"Invalid user GUID: {guidString}");

            var user = await _userRepository.GetUserByGuidAsync(userGuid.ToString());
            if (user == null) throw new KeyNotFoundException($"User with GUID {guidString} not found");

            userIds.Add(user.Id);
        }

        // Add current user if not already in the list
        var currentUser = await _userRepository.GetUserByGuidAsync(currentUserGuid.ToString());
        if (currentUser == null) throw new KeyNotFoundException("Current user not found");

        if (!userIds.Contains(currentUser.Id)) userIds.Add(currentUser.Id);

        // Check if the conversation is a group chat
        if (request.IsGroup && userIds.Count < 2) throw new ArgumentException("Group conversations must have at least 2 participants");
        
        // Check if the conversation is a 1:1 chat
        if (!request.IsGroup && userIds.Count > 2) throw new ArgumentException("1:1 conversations can only have 2 participants");
        
        // For 1:1 chat, check if conversation already exists
        if (!request.IsGroup && userIds.Count == 2)
        {
            var otherUserId = userIds.First(id => id != currentUser.Id);
            var existingConversation =
                await _conversationRepository.GetOneToOneConversationAsync(currentUser.Id, otherUserId);

            if (existingConversation != null)
                return await GetConversationResponseAsync(existingConversation.ConversationId);
        }

        // Create new conversation
        var conversation = new Conversation
        {
            ConversationId = Guid.NewGuid(),
            Name = request.Name,
            IsGroup = request.IsGroup,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            Members = new List<ConversationMember>()
        };

        // Add members to the conversation
        foreach (var userId in userIds)
        {
            var role = userId == currentUser.Id ? MemberRole.Admin : MemberRole.Regular;
            conversation.Members.Add(new ConversationMember
            {
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                Role = role
            });
        }

        // Save to database
        await _conversationRepository.CreateConversationAsync(conversation);

        // Get response
        var response = await GetConversationResponseAsync(conversation.ConversationId);
        
        // Notify participants about new conversation
        await NotifyParticipantsAsync(conversation, "ConversationCreated", response);

        return response;
    }

    public async Task<List<ConversationResponse>> GetUserConversationsAsync(Guid userGuid)
    {
        var user = await _userRepository.GetUserByGuidAsync(userGuid.ToString());
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var conversations = await _conversationRepository.GetUserConversationAsync(user.Id);
        var response = new List<ConversationResponse>();

        foreach (var conversation in conversations) response.Add(await MapConversationToResponseAsync(conversation));

        return response;
    }

    public async Task<ConversationResponse> GetConversationByIdAsync(Guid conversationId, Guid userGuid)
    {
        var user = await _userRepository.GetUserByGuidAsync(userGuid.ToString());
        if (user == null) throw new KeyNotFoundException("User not found");

        var isUserInConversation = await _conversationRepository.IsUserInConversationAsync(conversationId, user.Id);
        if (!isUserInConversation) throw new UnauthorizedAccessException("User is not a member of this conversation");

        return await GetConversationResponseAsync(conversationId);
    }

    public async Task<bool> UpdateConversationTitleAsync(Guid conversationId, string newTitle, Guid userGuid)
    {
        var user = await _userRepository.GetUserByGuidAsync(userGuid.ToString());
        if (user == null) throw new KeyNotFoundException("User not found");

        var conversation = await _conversationRepository.GetConversationByIdAsync(conversationId);
        // Check if conversation exists and is a group
        if (conversation is not { IsGroup: true })
            throw new InvalidOperationException("Only group conversations can have their title changed");
        
        // Check if user is an admin of this conversation
        var member = conversation.Members.FirstOrDefault(m => m.UserId == user.Id);
        if (member == null || member.Role != MemberRole.Admin) throw new UnauthorizedAccessException("Only admins can change the conversation title");

        var success = await _conversationRepository.UpdateConversationTitleAsync(conversationId, newTitle);
        
        if (success)
        {
            // Get updated conversation
            var updatedConversation = await GetConversationResponseAsync(conversationId);
            
            // Notify participants
            await NotifyParticipantsAsync(conversation, "ConversationUpdated", updatedConversation);
        }
        
        return success;
    }

    public async Task<bool> AddUserToConversationAsync(Guid conversationId, string userToAddGuidString, Guid currentUserGuid)
    {
        if (!Guid.TryParse(userToAddGuidString, out var userToAddGuid))
            throw new ArgumentException($"Invalid user GUID: {userToAddGuidString}");

        var currentUser = await _userRepository.GetUserByGuidAsync(currentUserGuid.ToString());
        if (currentUser == null)
            throw new KeyNotFoundException("Current user not found");

        var userToAdd = await _userRepository.GetUserByGuidAsync(userToAddGuid.ToString());
        if (userToAdd == null)
            throw new KeyNotFoundException("User to add not found");

        var conversation = await _conversationRepository.GetConversationByIdAsync(conversationId);

        // Check if conversation is a group (can't add users to 1:1 chats)
        if (!conversation.IsGroup)
            throw new InvalidOperationException("Cannot add users to a one-to-one conversation");

        // Check if current user is admin
        var member = conversation.Members.FirstOrDefault(m => m.UserId == currentUser.Id);
        if (member == null || member.Role != MemberRole.Admin)
            throw new UnauthorizedAccessException("Only admins can add users to the conversation");

        var success = await _conversationRepository.AddUserToConversationAsync(conversationId, userToAdd.Id);
        
        if (success)
        {
            // Get updated conversation
            var updatedConversation = await GetConversationResponseAsync(conversationId);
            
            // Create user added event data
            var userAddedEvent = new 
            {
                ConversationId = conversationId,
                UserAdded = new UserBriefDto
                {
                    Id = userToAdd.UserId,
                    FullName = userToAdd.FirstName + " " + userToAdd.LastName,
                    ProfilePictureUrl = userToAdd.ProfilePictureUrl,
                    Role = MemberRole.Regular,
                    JoinedAt = DateTime.UtcNow
                },
                AddedBy = new UserBriefDto
                {
                    Id = currentUser.UserId,
                    FullName = currentUser.FirstName + " " + currentUser.LastName
                }
            };
            
            // Notify existing participants
            await NotifyParticipantsAsync(conversation, "UserAddedToConversation", userAddedEvent);
            
            // Add new user to SignalR group
            await _chatHubContext.Groups.AddToGroupAsync(userToAdd.UserId.ToString(), $"conversation-{conversationId}");
            
            // Send the full conversation to the new user
            await _chatHubContext.Clients.User(userToAdd.UserId.ToString()).SendAsync("ConversationCreated", updatedConversation);
        }
        
        return success;
    }

    public async Task<bool> RemoveUserFromConversationAsync(Guid conversationId, string userToRemoveGuidString, Guid currentUserGuid)
    {
        if (!Guid.TryParse(userToRemoveGuidString, out var userToRemoveGuid))
            throw new ArgumentException($"Invalid user GUID: {userToRemoveGuidString}");

        var currentUser = await _userRepository.GetUserByGuidAsync(currentUserGuid.ToString());
        if (currentUser == null)
            throw new KeyNotFoundException("Current user not found");

        var userToRemove = await _userRepository.GetUserByGuidAsync(userToRemoveGuid.ToString());
        if (userToRemove == null)
            throw new KeyNotFoundException("User to remove not found");

        var conversation = await _conversationRepository.GetConversationByIdAsync(conversationId);

        // Can't remove users from 1:1 chats
        if (!conversation.IsGroup)
            throw new InvalidOperationException("Cannot remove users from a one-to-one conversation");

        // Check permissions: either self-removal or admin removing someone
        var isSelfRemoval = userToRemove.Id == currentUser.Id;
        var isAdminRemoval = conversation.Members.Any(m => m.UserId == currentUser.Id && m.Role == MemberRole.Admin);

        if (!isSelfRemoval && !isAdminRemoval)
            throw new UnauthorizedAccessException("Only admins can remove other users from the conversation");

        var success = await _conversationRepository.RemoveUserFromConversationAsync(conversationId, userToRemove.Id);
        
        if (success)
        {
            // Create user removed event data
            var userRemovedEvent = new 
            {
                ConversationId = conversationId,
                UserRemoved = new 
                {
                    Id = userToRemove.UserId,
                    FullName = userToRemove.FirstName + " " + userToRemove.LastName
                },
                RemovedBy = new 
                {
                    Id = currentUser.UserId,
                    FullName = currentUser.FirstName + " " + currentUser.LastName
                },
                IsSelfRemoval = isSelfRemoval
            };
            
            // Notify participants
            await NotifyParticipantsAsync(conversation, "UserRemovedFromConversation", userRemovedEvent);
            
            // Remove user from SignalR group
            await _chatHubContext.Groups.RemoveFromGroupAsync(userToRemove.UserId.ToString(), $"conversation-{conversationId}");
            
            // Notify the removed user
            if (!isSelfRemoval)
            {
                await _chatHubContext.Clients.User(userToRemove.UserId.ToString()).SendAsync("RemovedFromConversation", 
                    new { ConversationId = conversationId });
            }
        }
        
        return success;
    }

    public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid currentUserGuid)
    {
        var currentUser = await _userRepository.GetUserByGuidAsync(currentUserGuid.ToString());
        if (currentUser == null)
            throw new KeyNotFoundException("Current user not found");

        var conversation = await _conversationRepository.GetConversationByIdAsync(conversationId);

        // Check if user is an admin of this conversation
        var member = conversation.Members.FirstOrDefault(m => m.UserId == currentUser.Id);
        if (member == null || member.Role != MemberRole.Admin)
            throw new UnauthorizedAccessException("Only admins can delete the conversation");

        // Before deletion, create deletion event and capture participants
        var deletionEvent = new 
        {
            ConversationId = conversationId,
            DeletedBy = new 
            {
                Id = currentUser.UserId,
                FullName = currentUser.FirstName + " " + currentUser.LastName
            },
            DeletedAt = DateTime.UtcNow
        };
        
        // Get all participants to notify them
        var participants = conversation.Members.ToList(); // Create a copy
        
        var success = await _conversationRepository.DeleteConversationAsync(conversationId);
        
        if (success)
        {
            // Notify all participants
            foreach (var participant in participants)
            {
                var user = await _userRepository.GetUserByIdAsync(participant.UserId);
                if (user != null)
                {
                    await _chatHubContext.Clients.User(user.UserId.ToString())
                        .SendAsync("ConversationDeleted", deletionEvent);
                }
            }
        }
        
        return success;
    }

    // Helper methods
    private async Task<ConversationResponse> GetConversationResponseAsync(Guid conversationId)
    {
        var conversation = await _conversationRepository.GetConversationByIdAsync(conversationId);
        return await MapConversationToResponseAsync(conversation);
    }

    private async Task<ConversationResponse> MapConversationToResponseAsync(Conversation conversation)
    {
        var participants = new List<UserBriefDto>();
        foreach (var member in conversation.Members)
            participants.Add(new UserBriefDto
            {
                Id = member.User.UserId,
                FullName = member.User.FirstName + " " + member.User.LastName,
                Role = member.Role,
                JoinedAt = member.JoinedAt,
                ProfilePictureUrl = member.User.ProfilePictureUrl
            });

        MessageDto? latestMessage = null;
        if (conversation.Messages != null && conversation.Messages.Any())
        {
            var message = conversation.Messages.OrderByDescending(m => m.SentAt).First();
            var sender = await _userRepository.GetUserByIdAsync(message.SenderId);

            latestMessage = new MessageDto
            {
                Id = message.Id,
                MessageId = message.MessageId,
                ConversationId = message.Conversation.ConversationId,
                SenderId = message.SenderId,
                SenderName = message.Sender?.FirstName + " " + message.Sender?.LastName ?? "Unknown User",
                SenderProfilePicture = sender?.ProfilePictureUrl,
                Content = message.Content,
                SentAt = message.SentAt,
                IsEdited = message.IsEdited,
                EditedAt = message.EditedAt,
                IsDeleted = message.IsDeleted,
                ReplyToMessageId = message.ReplyToMessageId
            };
        }

        return new ConversationResponse
        {
            ConversationId = conversation.ConversationId,
            Name = conversation.Name,
            IsGroup = conversation.IsGroup,
            LastMessageAt = conversation.LastMessageAt,
            Participants = participants,
            LatestMessage = latestMessage
        };
    }
    
    // Helper method to notify conversation participants about changes
    private async Task NotifyParticipantsAsync<T>(Conversation conversation, string eventName, T eventData)
    {
        try
        {
            // Send to the conversation group
            await _chatHubContext.Clients.Group($"conversation-{conversation.ConversationId}")
                .SendAsync(eventName, eventData);
            
            // Additionally, send to each participant individually (useful for users with multiple devices)
            foreach (var member in conversation.Members)
            {
                var user = await _userRepository.GetUserByIdAsync(member.UserId);
                if (user != null)
                {
                    await _chatHubContext.Clients.User(user.UserId.ToString())
                        .SendAsync(eventName, eventData);
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the operation
            Console.WriteLine($"Error notifying participants: {ex.Message}");
        }
    }
}