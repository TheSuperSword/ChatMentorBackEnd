using System.ComponentModel.DataAnnotations;

namespace ChatMentor.Backend.Model;

public class AuditableEntity
{
    [StringLength(50)]
    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(50)]
    public Guid? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}