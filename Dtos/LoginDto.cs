using System.ComponentModel.DataAnnotations;

namespace Othello_API.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;   // Enforces a valid email format

        public string? UserName { get; set; }  // Optional: Allow login with username

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = string.Empty;  // Enforces minimum password length

        // Custom validation: Ensure either Email or UserName is provided
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(UserName);
        }
    }
}
