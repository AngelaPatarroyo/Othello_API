public class UpdateGameDto
{
    public required string GameStatus { get; set; }  
    public string? WinnerId { get; set; }  // Nullable since there might be no winner yet
}
