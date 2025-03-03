using Microsoft.EntityFrameworkCore;

public class GameRepository : IGameRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GameRepository> _logger;

    public GameRepository(ApplicationDbContext context, ILogger<GameRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Game?> GetByIdAsync(int gameId)
    {
        _logger.LogInformation("Fetching game with ID {GameId} from the database.", gameId);

        var game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found.", gameId);
        }

        return game;
    }

    public async Task<List<Game>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all games from the database.");

        var games = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .ToListAsync();

        _logger.LogInformation("Successfully retrieved {GameCount} games.", games.Count);

        return games;
    }

    public async Task AddAsync(Game game)
    {
        _logger.LogInformation("Adding a new game to the database with Player1: {Player1Id} and Player2: {Player2Id}.", game.Player1Id, game.Player2Id);

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Game with ID {GameId} added successfully.", game.GameId);
    }

    public async Task UpdateAsync(Game game)
    {
        _logger.LogInformation("Updating game with ID {GameId}.", game.GameId);

        _context.Games.Update(game);
        _context.Entry(game).Property(g => g.WinnerId).IsModified = true;  // Ensures WinnerId is saved
        await _context.SaveChangesAsync();

        _logger.LogInformation("Game with ID {GameId} updated successfully.", game.GameId);
    }

    public async Task<bool> DeleteAsync(int gameId)
    {
        _logger.LogInformation("Attempting to delete game with ID {GameId}.", gameId);

        var game = await _context.Games.FindAsync(gameId);
        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found for deletion.", gameId);
            return false;
        }

        _context.Games.Remove(game);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Game with ID {GameId} deleted successfully.", gameId);
        return true;
    }

    // Leaderboard Handling

    public async Task<LeaderBoard?> GetLeaderboardEntryByPlayerIdAsync(string playerId)
    {
        _logger.LogInformation("Fetching leaderboard entry for PlayerId: {PlayerId}.", playerId);

        var leaderboardEntry = await _context.LeaderBoard.FirstOrDefaultAsync(lb => lb.PlayerId == playerId);

        if (leaderboardEntry == null)
        {
            _logger.LogWarning("Leaderboard entry for PlayerId {PlayerId} not found.", playerId);
        }

        return leaderboardEntry;
    }

    public async Task AddLeaderboardEntryAsync(LeaderBoard leaderboard)
    {
        _logger.LogInformation("Adding leaderboard entry for PlayerId: {PlayerId}.", leaderboard.PlayerId);

        _context.LeaderBoard.Add(leaderboard);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Leaderboard entry for PlayerId {PlayerId} added successfully.", leaderboard.PlayerId);
    }

    public async Task UpdateLeaderboardAsync(LeaderBoard leaderboard)
    {
        _logger.LogInformation("Updating leaderboard entry for PlayerId: {PlayerId}.", leaderboard.PlayerId);

        _context.LeaderBoard.Update(leaderboard);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Leaderboard entry for PlayerId {PlayerId} updated successfully.", leaderboard.PlayerId);
    }
}      
