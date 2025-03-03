using Microsoft.EntityFrameworkCore;


public class LeaderBoardService : ILeaderBoardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeaderBoardService> _logger;

    public LeaderBoardService(ApplicationDbContext context, ILogger<LeaderBoardService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Fetch full leaderboard
    public async Task<List<LeaderBoardDto>> GetLeaderboardAsync()
    {
        _logger.LogInformation("Fetching full leaderboard.");

        var leaderboard = await _context.LeaderBoard
            .Include(l => l.Player)
            .Where(l => l.Player != null)
            .Select(lb => new LeaderBoardDto
            {
                PlayerId = lb.PlayerId,
                PlayerName = lb.Player.UserName ?? "Unknown",
                Wins = lb.Wins
            })
            .OrderByDescending(l => l.Wins)
            .ToListAsync();

        if (leaderboard.Count == 0)
        {
            _logger.LogWarning("No leaderboard data found.");
        }
        else
        {
            _logger.LogInformation("Fetched {LeaderboardCount} leaderboard entries.", leaderboard.Count);
        }

        return leaderboard;
    }

    // Fetch ranking of a specific user
    public async Task<LeaderBoardDto?> GetUserRankingAsync(string userId)
    {
        _logger.LogInformation("Fetching ranking for UserId: {UserId}.", userId);

        var leaderboard = await GetLeaderboardAsync(); // Get full leaderboard
        var userRanking = leaderboard.FirstOrDefault(l => l.PlayerId == userId);

        if (userRanking == null)
        {
            _logger.LogWarning("User with ID {UserId} not found in leaderboard.", userId);
        }
        else
        {
            _logger.LogInformation("User with ID {UserId} found in leaderboard at position {Position}.", userId, leaderboard.IndexOf(userRanking) + 1);
        }

        return userRanking;
    }
}
