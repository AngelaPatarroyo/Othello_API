using System.ComponentModel.DataAnnotations;

public class UpdateGameDto
{
    [Required(ErrorMessage = "GameStatus is required.")]
    [RegularExpression("^(ongoing|finished|cancelled)$", ErrorMessage = "GameStatus must be 'ongoing', 'finished', or 'cancelled'.")]
    public required string GameStatus { get; set; }  

    [MinLength(1, ErrorMessage = "WinnerId cannot be an empty string.")]
    public string? WinnerId { get; set; }  // Nullable since there might be no winner yet

    // Optional: Add any other fields you might want to update, for example, Result
    public string? Result { get; set; }  // Nullable, in case the result isn't available when updating
}
