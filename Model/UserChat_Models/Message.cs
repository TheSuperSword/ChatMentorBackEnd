using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Model.UserChat_Models;

public class Message : AuditableEntity
{
    [Key] public int Id { get; set; }
    
    [Required] public Guid MessageId { get; set; } = Guid.NewGuid();
    
    public int ConversationId { get; set; } // Foreign key to Conversation
    
    public int SenderId { get; set; } // Foreign key to User (sender)
    
    [Required] [StringLength(4000)] public required string Content { get; set; }
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    public int? ReplyToMessageId { get; set; } // Self-reference to another message (for replies)
    
    public bool IsEdited { get; set; } = false;
    
    public DateTime? EditedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    [ForeignKey("ConversationId")] public Conversation? Conversation { get; set; }
    [ForeignKey("SenderId")] public User? Sender { get; set; }
    [ForeignKey("ReplyToMessageId")] public Message? ReplyToMessage { get; set; }
}
