using System.ComponentModel.DataAnnotations;

namespace ChatMentor.Backend.AuthModel
{
    public class AuthRequest
    {
        [Required]
        [StringLength(256)]
        public required string Email { get; set; }  // Email-based login

        [Required]
        [StringLength(256)]
        public required string Password { get; set; }  // Raw password (hashed before storage)
    }

    public class AuthResponse
    {
        public required string Token { get; set; }  // JWT or session token
        public DateTime Expiry { get; set; }
    }
}

namespace ChatMentor.Backend.PassChange
{
    public class PasswordChangeRequest
    {
        [Required]
        public required string CurrentPassword { get; set; }  // Needed for verification

        [Required]
        [StringLength(256)]
        public required string NewPassword { get; set; }

        [Required]
        [StringLength(256)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmNewPassword { get; set; }
    }
}