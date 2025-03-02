using System.ComponentModel.DataAnnotations;

public class UpdateGameDto
{
    [Required(ErrorMessage = "GameStatus is required.")]
    [RegularExpression("^(ongoing|finished|cancelled)$", ErrorMessage = "GameStatus must be 'ongoing', 'finished', or 'cancelled'.")]
    public required string GameStatus { get; set; }  

    [MinLength(1, ErrorMessage = "WinnerId cannot be an empty string.")]
    public string? WinnerId { get; set; }  // Nullable since there might be no winner yet
}
