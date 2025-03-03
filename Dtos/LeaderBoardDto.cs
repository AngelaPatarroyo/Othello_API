using System.ComponentModel.DataAnnotations;

public class LeaderBoardDto
{
    [Required(ErrorMessage = "PlayerId is required.")]
    [MinLength(1, ErrorMessage = "PlayerId cannot be empty.")]
    public required string PlayerId { get; set; }

    [Required(ErrorMessage = "PlayerName is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "PlayerName must be between 2 and 50 characters.")]
    public required string PlayerName { get; set; }  

    [Range(0, int.MaxValue, ErrorMessage = "Wins cannot be negative.")]
    public int Wins { get; set; } = 0;  // Default value is 0
}
