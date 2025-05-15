using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMentor.Backend.Model.UserChat_Models;

public class UserConnection : AuditableEntity
{
    [Key] public int Id { get; set; }
    
    [Required] [StringLength(128)] public required string ConnectionId { get; set; } // SignalR connection ID
    
    public int UserId { get; set; } // Foreign key to User
    
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(255)] public string? DeviceInfo { get; set; }
    
    [StringLength(45)] public string? IpAddress { get; set; }
    
    // Navigation property
    [ForeignKey("UserId")] public User? User { get; set; }
}