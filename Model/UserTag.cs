using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Model
{
    [Index(nameof(UserId), nameof(TagId), IsUnique = true)] // Ensure unique user-tag pair
    public class UserTag
    {
        [Key]
        public int Id { get; set; }  // Primary key for internal use

        [Required]
        public int UserId { get; set; }  // Foreign key (User)

        [Required]
        public int TagId { get; set; }  // Foreign key (Tag)

        // Navigation Properties
        public User? User { get; set; }  
        public Tag? Tag { get; set; }  

        // Audit Fields
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [StringLength(50)]
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}