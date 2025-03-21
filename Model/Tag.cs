using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Model
{
    [Index(nameof(Name), IsUnique = true)] // Prevent duplicate tags
    public class Tag 
    {
        [Key]
        public int Id { get; set; }  // Auto-incremented primary key

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }  // Name of the tag (e.g., "Software Engineer", "AI Specialist")

        // Navigation Property
        public List<UserTag> UserTags { get; set; } = [];

        // Audit Fields
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}