

public interface ILeaderBoardService
{
    Task<List<LeaderboardDto>> GetLeaderboardAsync();  // Fetch full leaderboard
    Task<LeaderboardDto?> GetUserRankingAsync(string userId);  // Fetch a specific user's ranking
}
