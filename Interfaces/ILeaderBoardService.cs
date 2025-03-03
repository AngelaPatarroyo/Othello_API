

public interface ILeaderBoardService
{
    Task<List<LeaderBoardDto>> GetLeaderboardAsync();  // Fetch full leaderboard
    Task<LeaderBoardDto?> GetUserRankingAsync(string userId);  // Fetch a specific user's ranking
}
