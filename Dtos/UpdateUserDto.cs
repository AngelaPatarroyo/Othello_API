using System.ComponentModel.DataAnnotations;

namespace Othello_API.Dtos
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "UserName must be between 2 and 50 characters.")]
        public string? UserName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long.")]
        public string? NewPassword { get; set; }

        // Ensure at least one field is provided
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserName) ||
                   !string.IsNullOrWhiteSpace(Email) ||
                   !string.IsNullOrWhiteSpace(NewPassword);
        }

        // Restrict updates to only allowed fields
        public void RestrictUpdates()
        {
            UserName = string.IsNullOrWhiteSpace(UserName) ? null : UserName.Trim();
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim();
            NewPassword = string.IsNullOrWhiteSpace(NewPassword) ? null : NewPassword.Trim();
        }
    }
}

