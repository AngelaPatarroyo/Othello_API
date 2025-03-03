public class GameDto
{
    public int GameId { get; set; }
    public string GameStatus { get; set; }
    public string CreatedAt { get; set; }
    public string? Player1Id { get; set; }
    public string? Player2Id { get; set; }
    public string? WinnerId { get; set; }
    public string? Result { get; set; }

    public GameDto(Game game)
    {
        GameId = game.GameId;
        GameStatus = game.GameStatus;
        CreatedAt = game.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        Player1Id = game.Player1Id;
        Player2Id = game.Player2Id;
        WinnerId = game.WinnerId;
        Result = game.Result;
    }
}
