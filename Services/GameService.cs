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

    // Create a game and associate players
    public async Task<Game> CreateGameAsync(StartGameDto gameDto)
    {
        // Ensure Player1 and Player2 exist in the database
        var player1 = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == gameDto.Player1Id);  // Fetch Player1

        var player2 = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == gameDto.Player2Id);  // Fetch Player2

        // If either player is not found, throw an exception
        if (player1 == null || player2 == null)
            throw new Exception("Players not found!");

        // Check if the usernames for players are empty or null
        if (string.IsNullOrWhiteSpace(player1.UserName) || string.IsNullOrWhiteSpace(player2.UserName))
        {
            throw new Exception("Players must have usernames associated with their account.");
        }

        // Create the game object and associate it with the players
        var game = new Game
        {
            Player1Id = gameDto.Player1Id,
            Player2Id = gameDto.Player2Id,
            Player1 = player1,  // Assign full Player1 object
            Player2 = player2,  // Assign full Player2 object
            GameStatus = "Ongoing",  // Set initial game status
            CreatedAt = DateTime.UtcNow  // Set creation time
        };

        // Add the new game to the context and save it
        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        return game;  // Return the created game
    }


    // Get game by ID
    public async Task<Game?> GetGameByIdAsync(int gameId)
    {
        return await _context.Games
            .Include(g => g.Player1)  // Include Player1 details
            .Include(g => g.Player2)  // Include Player2 details
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    // Get all games
    public async Task<List<Game>> GetAllGamesAsync()
    {
        return await _context.Games
            .Include(g => g.Player1)  // Include Player1 details
            .Include(g => g.Player2)  // Include Player2 details
            .ToListAsync();
    }

    // Update game status and winner if provided
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

    // Delete a game by ID
    public async Task<bool> DeleteGameAsync(int gameId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null) return false;

        _context.Games.Remove(game);
        await _context.SaveChangesAsync();
        return true;
    }
}
