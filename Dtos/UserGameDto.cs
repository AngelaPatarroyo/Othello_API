public class UserGameDto
{
    public required string UserId { get; set; }
    public required int GameId { get; set; }
    public int? TotalWins { get; set; } = 0;
    public int? TotalLosses { get; set; } = 0;
    public int? TotalGames { get; set; } = 0;
}
