using Microsoft.EntityFrameworkCore;


public class LeaderBoardService : ILeaderBoardService
{
    private readonly ApplicationDbContext _context;

    public LeaderBoardService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Fetch full leaderboard
    public async Task<List<LeaderboardDto>> GetLeaderboardAsync()
    {
        var leaderboard = await _context.LeaderBoard
            .Include(l => l.Player) 
            .Where(l => l.Player != null) 
            .Select(lb => new LeaderboardDto
            {
                PlayerId = lb.PlayerId,
                PlayerName = lb.Player.UserName ?? "Unknown",  
                Wins = lb.Wins
            })
            .OrderByDescending(l => l.Wins)  
            .ToListAsync();

        return leaderboard;
    }

    // Fetch ranking of a specific user
    public async Task<LeaderboardDto?> GetUserRankingAsync(string userId)
    {
        var leaderboard = await GetLeaderboardAsync(); //  Get full leaderboard
        return leaderboard.FirstOrDefault(l => l.PlayerId == userId);
    }
}
