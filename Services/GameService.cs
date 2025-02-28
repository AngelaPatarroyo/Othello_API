using Microsoft.EntityFrameworkCore;
using Othello_API.Models;
using Othello_API.Dtos;

public class GameService : IGameService
{
    private readonly ApplicationDbContext _context;

    public GameService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Game> CreateGameAsync(StartGameDto gameDto)
    {
        var game = new Game
        {
            Player1Id = gameDto.Player1Id,
            Player2Id = gameDto.Player2Id,
            GameStatus = "Ongoing",
            CreatedAt = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }


    public async Task<Game?> GetGameByIdAsync(int gameId)
    {
        return await _context.Games.FindAsync(gameId);
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        return await _context.Games.ToListAsync();
    }

    public async Task<bool> UpdateGameAsync(int gameId, UpdateGameDto dto)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null) return false;


        game.GameStatus = dto.Status ?? game.GameStatus;


        if (!string.IsNullOrEmpty(dto.WinnerId))
        {
            game.WinnerId = dto.WinnerId;
        }

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<bool> DeleteGameAsync(int gameId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null) return false;

        _context.Games.Remove(game);
        await _context.SaveChangesAsync();
        return true;
    }
}
