using Othello_API.DTOs;
using Microsoft.EntityFrameworkCore;


public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GameService> _logger;

    public GameService(IGameRepository gameRepository, IUserRepository userRepository, ApplicationDbContext context, ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<Game> CreateGameAsync(StartGameDto gameDto)
    {
        _logger.LogInformation("Attempting to create a game with Player1Id: {Player1Id} and Player2Id: {Player2Id}.", gameDto.Player1Id, gameDto.Player2Id);

        // Validate input
        if (gameDto == null || string.IsNullOrEmpty(gameDto.Player1Id) || string.IsNullOrEmpty(gameDto.Player2Id))
        {
            _logger.LogError("Player1Id or Player2Id is missing.");
            throw new ArgumentException("Player1Id and Player2Id are required.");
        }

        // Check if players exist
        var player1 = await _userRepository.GetByIdAsync(gameDto.Player1Id);
        var player2 = await _userRepository.GetByIdAsync(gameDto.Player2Id);

        if (player1 == null || player2 == null)
        {
            _logger.LogError("One or both players do not exist. Player1Id: {Player1Id}, Player2Id: {Player2Id}", gameDto.Player1Id, gameDto.Player2Id);
            throw new ArgumentException("One or both players do not exist.");
        }

        // Create game entity
        var game = new Game
        {
            Player1Id = gameDto.Player1Id,
            Player2Id = gameDto.Player2Id,
            GameStatus = gameDto.GameStatus ?? "Ongoing",
            Result = gameDto.Result,
            CreatedAt = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Game with ID {GameId} created successfully.", game.GameId);

        // Load Player1 and Player2 to ensure they are properly assigned
        game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .FirstOrDefaultAsync(g => g.GameId == game.GameId);

        if (game?.Player1 == null || game?.Player2 == null)
        {
            if (game == null)
            {
                _logger.LogError("Failed to associate players with game ID {GameId}.", gameDto.Player1Id);
            }
            else
            {
                _logger.LogError("Failed to associate players with game ID {GameId}.", game.GameId);
            }
            throw new Exception("Failed to associate players with the game.");
        }

        return game;
    }

    /// <summary>
    /// Retrieves a game by its ID.
    /// </summary>
    public async Task<Game?> GetGameByIdAsync(int gameId)
    {
        _logger.LogInformation("Fetching game with ID {GameId}.", gameId);
        var game = await _gameRepository.GetByIdAsync(gameId);

        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found.", gameId);
        }

        return game;
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        _logger.LogInformation("Fetching all games.");
        var games = await _gameRepository.GetAllAsync();

        _logger.LogInformation("Retrieved {GameCount} games.", games.Count);
        return games;
    }

    public async Task<bool> UpdateGameAsync(int gameId, UpdateGameDto dto)
    {
        _logger.LogInformation("Updating game with ID {GameId}.", gameId);

        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found for update.", gameId);
            return false;
        }

        game.GameStatus = dto.GameStatus;

        if (!string.IsNullOrEmpty(dto.WinnerId))
        {
            var winner = await _userRepository.GetByIdAsync(dto.WinnerId);
            if (winner != null && (winner.Id == game.Player1Id || winner.Id == game.Player2Id))
            {
                game.Winner = winner;
                game.WinnerId = winner.Id;

                // Update the Leaderboard logic
                var leaderboardEntry = await _gameRepository.GetLeaderboardEntryByPlayerIdAsync(winner.Id);
                if (leaderboardEntry != null)
                {
                    leaderboardEntry.Wins++; // Increase win count
                    await _gameRepository.UpdateLeaderboardAsync(leaderboardEntry);
                }
                else
                {
                    var newLeaderboardEntry = new LeaderBoard
                    {
                        PlayerId = winner.Id,
                        Wins = 1,
                        Player = winner
                    };
                    await _gameRepository.AddLeaderboardEntryAsync(newLeaderboardEntry);
                }

                _logger.LogInformation("Game with ID {GameId} has been updated with winner {WinnerId}.", gameId, winner.Id);
            }
        }

        await _gameRepository.UpdateAsync(game);
        return true;
    }

    public async Task<bool> DeleteGameAsync(int gameId)
    {
        _logger.LogInformation("Deleting game with ID {GameId}.", gameId);
        var success = await _gameRepository.DeleteAsync(gameId);

        if (success)
        {
            _logger.LogInformation("Game with ID {GameId} deleted successfully.", gameId);
        }
        else
        {
            _logger.LogWarning("Game with ID {GameId} not found for deletion.", gameId);
        }

        return success;
    }
}
