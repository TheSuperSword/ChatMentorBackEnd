using System.ComponentModel.DataAnnotations;

namespace ChatMentor.Backend.DTOs;

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateTagDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateTagDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}