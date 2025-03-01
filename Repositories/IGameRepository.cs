

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(int gameId);
    Task<List<Game>> GetAllAsync();
    Task AddAsync(Game game);
    Task UpdateAsync(Game game);
    Task<bool> DeleteAsync(int gameId);
    Task<LeaderBoard?> GetLeaderboardEntryByPlayerIdAsync(string playerId);
    Task AddLeaderboardEntryAsync(LeaderBoard leaderboard);
    Task UpdateLeaderboardAsync(LeaderBoard leaderboard);
}
