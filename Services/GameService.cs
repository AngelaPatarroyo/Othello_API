using Othello_API.DTOs;
using Microsoft.EntityFrameworkCore;
using Othello_API.Models;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IGameRepository gameRepository,
        IUserRepository userRepository,
        ApplicationDbContext context,
        ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<Game> CreateGameAsync(StartGameDto gameDto)
    {
        _logger.LogInformation("Creating game with Player1Id: {Player1Id}, Player2Id: {Player2Id}", gameDto.Player1Id, gameDto.Player2Id);

        if (string.IsNullOrEmpty(gameDto.Player1Id) || string.IsNullOrEmpty(gameDto.Player2Id))
            throw new ArgumentException("Player1Id and Player2Id are required.");

        var player1 = await _userRepository.GetByIdAsync(gameDto.Player1Id);
        var player2 = await _userRepository.GetByIdAsync(gameDto.Player2Id);

        if (player1 == null || player2 == null)
            throw new ArgumentException("One or both players do not exist.");

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

        return await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .FirstOrDefaultAsync(g => g.GameId == game.GameId)
            ?? throw new Exception("Failed to retrieve created game.");
    }

    public async Task<Game?> GetGameByIdAsync(int gameId)
    {
        _logger.LogInformation("Fetching game with ID {GameId}", gameId);
        return await _gameRepository.GetByIdAsync(gameId);
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        _logger.LogInformation("Fetching all games");
        return await _gameRepository.GetAllAsync();
    }

    public async Task<bool> UpdateGameAsync(int gameId, UpdateGameDto dto)
    {
        _logger.LogInformation("Updating game ID {GameId}", gameId);

        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            _logger.LogWarning("Game ID {GameId} not found", gameId);
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

                // Leaderboard update
                var leaderboard = await _gameRepository.GetLeaderboardEntryByPlayerIdAsync(winner.Id);
                if (leaderboard != null)
                {
                    leaderboard.Wins++;
                    await _gameRepository.UpdateLeaderboardAsync(leaderboard);
                }
                else
                {
                    await _gameRepository.AddLeaderboardEntryAsync(new LeaderBoard
                    {
                        PlayerId = winner.Id,
                        Wins = 1,
                        Player = winner
                    });
                }

                // Track both players in UserGames
                var loserId = (winner.Id == game.Player1Id) ? game.Player2Id : game.Player1Id;

                _context.UserGames.AddRange(
                    new UserGame { UserId = winner.Id, GameId = game.GameId, IsWinner = true },
                    new UserGame { UserId = loserId, GameId = game.GameId, IsWinner = false }
                );

                await _context.SaveChangesAsync();
                _logger.LogInformation("UserGames updated for game ID {GameId}", game.GameId);
            }
        }

        await _gameRepository.UpdateAsync(game);
        return true;
    }

    public async Task<bool> DeleteGameAsync(int gameId)
    {
        _logger.LogInformation("Deleting game ID {GameId}", gameId);
        return await _gameRepository.DeleteAsync(gameId);
    }

    public void GetGameById(int gameId)
    {
        throw new NotImplementedException();
    }
}
