namespace Othello_API.DTOs
{
    public class StartGameDto
    {
        public required string Player1Id { get; set; }
        public required string Player2Id { get; set; }
        public string? WinnerId { get; set; }
        public string GameStatus { get; set; } = "ongoing";
        public string? Result { get; set; }
    }
}
