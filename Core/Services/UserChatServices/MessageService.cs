using System.Diagnostics;
using ChatMentor.Backend.ChatHubs;
using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.Core.Repositories.UserChatFeature;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model;
using ChatMentor.Backend.Model.UserChat_Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace ChatMentor.Backend.Core.Services;

public class MessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly DocumentService _documentService;
    private readonly IMessageAttachmentRepository _messageAttachmentRepository;
    private readonly IUserConnectionsRepository _userConnectionsRepository;
    private readonly IHubContext<ChatHub> _chatHubContext;

    public MessageService(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        DocumentService documentService,
        IMessageAttachmentRepository messageAttachmentRepository, IHubContext<ChatHub> chatHubContext, IUserConnectionsRepository userConnectionsRepository)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _documentService = documentService;
        _messageAttachmentRepository = messageAttachmentRepository;
        _chatHubContext = chatHubContext;
        _userConnectionsRepository = userConnectionsRepository;
    }
    public async Task<MessageDto> SendMessageAsync(MessageRequest request, Guid currentUserGuid)
    {
        // Validate user
        var currentUser = await _userRepository.GetUserByGuidAsync(currentUserGuid.ToString());
        if (currentUser == null)
            throw new KeyNotFoundException("Current user not found");

        // Validate conversation and user membership
        var conversation = await _conversationRepository.GetConversationByIdAsync(request.ConversationId);
        var isUserInConversation = conversation.Members.Any(m => m.UserId == currentUser.Id);

        if (!isUserInConversation)
            throw new UnauthorizedAccessException("User is not a member of this conversation");

        // Validate reply-to message if provided
        Message? replyToMessage = null;
        if (request.ReplyToMessageId.HasValue)
        {
            var replyMessage =
                await _messageRepository.GetMessageByGuidAsync(new Guid(request.ReplyToMessageId.Value.ToString()));
            if (replyMessage == null)
                throw new KeyNotFoundException("Reply message not found");

            // Ensure the reply message belongs to the same conversation
            if (replyMessage.ConversationId != conversation.Id)
                throw new InvalidOperationException("Cannot reply to a message from a different conversation");

            replyToMessage = replyMessage;
        }

        // Create and save the message
        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = currentUser.Id,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsEdited = false,
            IsDeleted = false,
            ReplyToMessageId = replyToMessage?.Id
        };

        await _messageRepository.CreateMessageAsync(message);

        // Process attachments if any
        var attachmentDtos = new List<AttachmentDto>();
        if (request.Attachments != null && request.Attachments.Count > 0)
        {
            foreach (var file in request.Attachments)
            {
                try
                {
                    // Upload file using DocumentService
                    var document = await _documentService.UploadDocumentAsync(
                        file, currentUserGuid,
                        "chat_attachments", conversation.ConversationId);

                    if (document != null)
                    {
                        // Create message attachment
                        var attachment = new MessageAttachment
                        {
                            MessageId = message.Id,
                            DocumentId = document.Id,
                            CreatedBy = currentUserGuid,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _messageAttachmentRepository.CreateAttachmentAsync(attachment);

                        // Add to DTOs
                        attachmentDtos.Add(new AttachmentDto
                        {
                            AttachmentId = document.DocId,
                            FileName = document.FileName,
                            ContentType = document.ContentType,
                            FileSize = document.FileSize,
                            DownloadUrl = $"/api/userchat/download/{document.DocId}"
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other attachments
                    Console.WriteLine($"Error uploading attachment: {ex.Message}");
                }
            }
        }

        // Update conversation's LastMessageAt timestamp
        await _conversationRepository.UpdateLastUpdatedAtAsync(request.ConversationId);

        // Create the message DTO for response and notifications
        var messageDto = new MessageDto
        {
            Id = message.Id,
            MessageId = message.MessageId,
            ConversationId = message.Conversation.ConversationId,
            SenderId = message.SenderId,
            SenderName = currentUser.FirstName,
            SenderProfilePicture = currentUser.ProfilePictureUrl,
            Content = message.Content,
            SentAt = message.SentAt,
            IsEdited = message.IsEdited,
            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted,
            ReplyToMessageId = message.ReplyToMessageId,
            Attachments = attachmentDtos
        };

        // Notify all users in the conversation via SignalR
        await NotifyConversationMembersAsync(conversation, messageDto);

        // Return DTO
        return messageDto;
    }

    public async Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId, Guid currentUserGuid, Guid? cursorMessageId = null, int limit = 20)
    {
        // Validate user
        var currentUser = await _userRepository.GetUserByGuidAsync(currentUserGuid.ToString());
        if (currentUser == null)
            throw new KeyNotFoundException("Current user not found");

        // Check if user is in conversation
        var isUserInConversation =
            await _conversationRepository.IsUserInConversationAsync(conversationId, currentUser.Id);
        if (!isUserInConversation)
            throw new UnauthorizedAccessException("User is not a member of this conversation");

        // Get messages using cursor-based pagination
        var messages = await _messageRepository.GetMessagesByConversationIdAsync(conversationId, cursorMessageId, limit);
        var messageDtos = new List<MessageDto>();

        foreach (var message in messages) 
            messageDtos.Add(await MapMessageToDto(message));

        return messageDtos;
    }

    public async Task<bool> EditMessageAsync(Guid messageId, string newContent, Guid currentUserGuid)
    {
        // Validate user
        var currentUser = await _userRepository.GetUserByGuidAsync(currentUserGuid.ToString());
        if (currentUser == null) throw new KeyNotFoundException("Current user not found");

        // Get a message
        var message = await _messageRepository.GetMessageByGuidAsync(messageId);
        if (message == null) throw new KeyNotFoundException("Message not found");

        // Check if the user is the sender of the message
        if (message.SenderId != currentUser.Id) throw new UnauthorizedAccessException("Only the sender can edit their messages");

        // Update message
        message.Content = newContent;
        var success = await _messageRepository.UpdateMessageAsync(message);
        if (!success) throw new InvalidOperationException("Failed to update message");

        // Get a message that needs to be updated
        var updatedMessage = await _messageRepository.GetMessageByGuidAsync(messageId);
        if (updatedMessage == null) throw new KeyNotFoundException("Failed to retrieve updated message");

        // Map to DTO and notify via SignalR
        var messageDto = await MapMessageToDto(updatedMessage);
        Debug.Assert(updatedMessage.Conversation != null, "updatedMessage.Conversation != null");
        await _chatHubContext.Clients.Group($"conversation-{updatedMessage.Conversation.ConversationId}").SendAsync("MessageUpdated", messageDto);
        return true;
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid currentUserGuid)
    {
        // Validate user
        var currentUser = await _userRepository.GetUserByGuidAsync(currentUserGuid.ToString());
        if (currentUser == null) throw new KeyNotFoundException("Current user not found");

        // Get message
        var message = await _messageRepository.GetMessageByGuidAsync(messageId);
        if (message == null) throw new KeyNotFoundException("Message not found");

        // Get conversation to check if user is admin
        var conversation = await _conversationRepository.GetConversationByIdAsync(
            (await _conversationRepository.GetConversationByIdAsync(message.Conversation.ConversationId))
            .ConversationId);

        var member = conversation.Members.FirstOrDefault(m => m.UserId == currentUser.Id);

        // Check if user is the sender or an admin
        var isSender = message.SenderId == currentUser.Id;
        var isAdmin = member?.Role == MemberRole.Admin;

        if (!isSender && !isAdmin)
            throw new UnauthorizedAccessException("Only the sender or conversation admin can delete messages");

        // Delete message
        var success = await _messageRepository.DeleteMessageAsync(messageId);
        
        if (success)
        {
            // Notify via SignalR that message was deleted
            await _chatHubContext.Clients.Group($"conversation-{conversation.ConversationId}")
                .SendAsync("MessageDeleted", messageId);
        }
        
        return success;
    }
    
    
    // New method to notify conversation members via SignalR
    // Modified method to exclude the sender from notifications
    private async Task NotifyConversationMembersAsync(Conversation conversation, MessageDto messageDto)
    {
        try
        {
            // Get all conversation members
            var members = conversation.Members;
        
            // For each member, get their active connections
            foreach (var member in members)
            {
                // Skip the sender to avoid duplicate messages
                if (member.UserId == messageDto.SenderId)
                    continue;
                
                // Get user's active connections
                var userGuid = (await _userRepository.GetUserByIdAsync(member.UserId))?.UserId.ToString();
                if (string.IsNullOrEmpty(userGuid)) continue;
            
                // Send message to each connection
                await _chatHubContext.Clients.User(userGuid).SendAsync("ReceiveMessage", messageDto);
            }
        
            // For the group notification, you have two options:
        
            // Option 1: Remove this line completely if you're using User-specific notifications above
            // This avoids any chance of duplicate messages
        
            // Option 2: If you need to keep group notifications for some reason,
            // you could use a different event name to distinguish it:
            // await _chatHubContext.Clients.Group($"conversation-{conversation.ConversationId}")
            //     .SendAsync("ConversationUpdated", messageDto);
        
            // I recommend removing this line completely:
            // await _chatHubContext.Clients.Group($"conversation-{conversation.ConversationId}").SendAsync("ReceiveMessage", messageDto);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the message send operation
            Console.WriteLine($"Error notifying conversation members: {ex.Message}");
        }
    }
    
    // Helper method to map Message to MessageDto
    private async Task<MessageDto> MapMessageToDto(Message message)
    {
        var sender = await _userRepository.GetUserByIdAsync(message.SenderId);

        // Handle case where sender might be null (deleted user)
        var senderName = sender?.FirstName ?? "Unknown User";
        var senderProfilePicture = sender?.ProfilePictureUrl;

        // Build reply information if available
        int? replyToMessageId = null;
        if (message.ReplyToMessage != null) replyToMessageId = message.ReplyToMessage.Id;

        // Get attachments for this message
        var attachments = await _messageAttachmentRepository.GetAttachmentsByMessageIdAsync(message.Id);
        var attachmentDtos = new List<AttachmentDto>();

        foreach (var attachment in attachments)
        {
            if (attachment.Document != null)
            {
                attachmentDtos.Add(new AttachmentDto
                {
                    AttachmentId = attachment.Document.DocId,
                    FileName = attachment.Document.FileName,
                    ContentType = attachment.Document.ContentType,
                    FileSize = attachment.Document.FileSize,
                    DownloadUrl = $"/api/userchat/download/{attachment.Document.DocId}"
                });
            }
        }

        return new MessageDto
        {
            Id = message.Id,
            MessageId = message.MessageId,
            ConversationId = message.Conversation.ConversationId,
            SenderId = message.SenderId,
            SenderName = senderName,
            SenderProfilePicture = senderProfilePicture,
            Content = message.Content,
            SentAt = message.SentAt,
            IsEdited = message.IsEdited,
            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted,
            ReplyToMessageId = replyToMessageId,
            Attachments = attachmentDtos.Any() ? attachmentDtos : null
        };
    }
}