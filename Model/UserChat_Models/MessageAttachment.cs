using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMentor.Backend.Model.UserChat_Models;

public class MessageAttachment : AuditableEntity
{
    [Key] public int Id { get; set; }
    
    // Message this attachment belongs to
    public int MessageId { get; set; }
    
    // Document reference
    public int DocumentId { get; set; }
    
    // Navigation properties
    [ForeignKey("MessageId")] public Message? Message { get; set; }
    [ForeignKey("DocumentId")] public Document? Document { get; set; }
}