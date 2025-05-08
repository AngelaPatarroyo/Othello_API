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

    public async Task<List<LeaderBoardDto>> GetLeaderboardAsync()
    {
        _logger.LogInformation("Fetching full leaderboard.");

        var users = await _context.Users.ToListAsync();
        var games = await _context.Games.ToListAsync();

        var leaderboard = users.Select(user =>
        {
            var userGames = games.Where(g => g.Player1Id == user.Id || g.Player2Id == user.Id).ToList();
            int wins = userGames.Count(g => g.WinnerId == user.Id);
            int losses = userGames.Count(g => g.WinnerId != null && g.WinnerId != user.Id);
            int draws = userGames.Count(g => g.WinnerId == null);
            int total = userGames.Count;
            double winRate = total > 0 ? (double)wins / total * 100 : 0;

            return new LeaderBoardDto
            {
                PlayerId = user.Id,
                PlayerName = user.UserName ?? "Unknown",
                Wins = wins,
                Losses = losses,
                Draws = draws,
                TotalGames = total,
                WinRate = winRate
            };
        })
        .OrderByDescending(x => x.Wins)
        .ToList();

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

    public async Task<LeaderBoardDto?> GetUserRankingAsync(string userId)
    {
        _logger.LogInformation("Fetching ranking for UserId: {UserId}.", userId);

        var leaderboard = await GetLeaderboardAsync();
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
