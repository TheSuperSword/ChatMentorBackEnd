using ChatMentor.Backend.Model.UserChat_Models;
using NuGet.Protocol.Plugins;

namespace ChatMentor.Backend.DTOs;

public class CreateConversationRequest
{
    public string? Name { get; set; } // Optional - for group chat
    public bool IsGroup { get; set; }
    public required List<string> ParticipantUserGuids { get; set; }
}

public class ConversationResponse
{
    public Guid ConversationId { get; set; }
    public string? Name { get; set; }
    public bool IsGroup { get; set; }
    public DateTime LastMessageAt { get; set; }
    public List<UserBriefDto> Participants { get; set; } = [];
    public MessageDto? LatestMessage { get; set; }
}

public class UserBriefDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public MemberRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderProfilePicture { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    public int? ReplyToMessageId { get; set; }
    public List<AttachmentDto>? Attachments { get; set; }
}

public class MessageRequest
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ReplyToMessageId { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}

public class AttachmentDto
{
    public Guid AttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class UserConnectionResponse
{
    public string UserId { get; set; }
}

public class UserConnectionRequest
{
    public required string UserId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}

