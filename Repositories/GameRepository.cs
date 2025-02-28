using Microsoft.EntityFrameworkCore;
using Othello_API.Models;

public class GameRepository : IGameRepository
{
    private readonly ApplicationDbContext _context;

    public GameRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Game?> GetByIdAsync(int gameId)
    {
        return await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    public async Task<List<Game>> GetAllAsync()
    {
        return await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .ToListAsync();
    }

    public async Task AddAsync(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Game game)
    {
        _context.Games.Update(game);
        _context.Entry(game).Property(g => g.WinnerId).IsModified = true;  // Ensures WinnerId is saved
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int gameId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null) return false;

        _context.Games.Remove(game);
        await _context.SaveChangesAsync();
        return true;
    }

    // 🔹 Add Leaderboard Handling

    public async Task<LeaderBoard?> GetLeaderboardEntryByPlayerIdAsync(string playerId)
{
    return await _context.LeaderBoard.FirstOrDefaultAsync(lb => lb.PlayerId == playerId);
}

public async Task AddLeaderboardEntryAsync(LeaderBoard leaderboard)
{
    _context.LeaderBoard.Add(leaderboard);
    await _context.SaveChangesAsync();
}

public async Task UpdateLeaderboardAsync(LeaderBoard leaderboard)
{
    _context.LeaderBoard.Update(leaderboard);
    await _context.SaveChangesAsync();
}

}
