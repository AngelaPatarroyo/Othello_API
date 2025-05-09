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

        if (string.IsNullOrEmpty(game.Player1Id))
            throw new ArgumentException("Player1Id cannot be null or empty.");

        var player1 = await _userRepository.GetByIdAsync(game.Player1Id);
        var player2 = game.Player2Id != null 
            ? await _userRepository.GetByIdAsync(game.Player2Id) 
            : null;

        if (player1 == null || player2 == null)
            return false;

        if (!string.IsNullOrEmpty(dto.WinnerId))
        {
            game.WinnerId = dto.WinnerId;

            var winner = dto.WinnerId == player1.Id ? player1 : player2;
            var loser = dto.WinnerId == player1.Id ? player2 : player1;

            winner.Wins++;
            winner.TotalGames++;
            winner.WinRate = (double)winner.Wins / winner.TotalGames * 100;

            loser.Losses++;
            loser.TotalGames++;
            loser.WinRate = (double)loser.Wins / loser.TotalGames * 100;

            await _userRepository.UpdateAsync(winner);
            await _userRepository.UpdateAsync(loser);

            await EnsureLeaderboardEntryAsync(winner);
            await EnsureLeaderboardEntryAsync(loser);

            _context.UserGames.AddRange(
                new UserGame { UserId = winner.Id, GameId = game.GameId, IsWinner = true },
                new UserGame { UserId = loser.Id, GameId = game.GameId, IsWinner = false }
            );
        }
        else
        {
            player1.Draws++;
            player2.Draws++;

            player1.TotalGames++;
            player2.TotalGames++;

            player1.WinRate = (double)player1.Wins / player1.TotalGames * 100;
            player2.WinRate = (double)player2.Wins / player2.TotalGames * 100;

            await _userRepository.UpdateAsync(player1);
            await _userRepository.UpdateAsync(player2);

            await EnsureLeaderboardEntryAsync(player1);
            await EnsureLeaderboardEntryAsync(player2);

            _context.UserGames.AddRange(
                new UserGame { UserId = player1.Id, GameId = game.GameId, IsWinner = false },
                new UserGame { UserId = player2.Id, GameId = game.GameId, IsWinner = false }
            );
        }

        await _context.SaveChangesAsync();
        await _gameRepository.UpdateAsync(game);

        _logger.LogInformation("Updated stats and leaderboard for game ID {GameId}", game.GameId);
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

    private async Task EnsureLeaderboardEntryAsync(ApplicationUser user)
    {
        var entry = await _gameRepository.GetLeaderboardEntryByPlayerIdAsync(user.Id);
        if (entry == null)
        {
            await _gameRepository.AddLeaderboardEntryAsync(new LeaderBoard
            {
                PlayerId = user.Id,
                Wins = user.Wins,
                Player = user
            });
        }
        else
        {
            entry.Wins = user.Wins;
            await _gameRepository.UpdateLeaderboardAsync(entry);
        }
    }
}
