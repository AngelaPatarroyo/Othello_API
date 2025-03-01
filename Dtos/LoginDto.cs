using System.ComponentModel.DataAnnotations;

namespace Othello_API.Dtos
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;   // Required: Must be provided for login

        public string? UserName { get; set; }  // Optional: Allow login with either username or email

        [Required]
        public string Password { get; set; } = string.Empty;  // Required: Must be provided for login
    }
}
