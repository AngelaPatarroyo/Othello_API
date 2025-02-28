
using Othello_API.Models;
using Othello_API.Dtos;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;

    public GameService(IGameRepository gameRepository, IUserRepository userRepository)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
    }

    public async Task<Game> CreateGameAsync(StartGameDto gameDto)
    {
        var player1 = await _userRepository.GetByIdAsync(gameDto.Player1Id);
        var player2 = await _userRepository.GetByIdAsync(gameDto.Player2Id);

        if (player1 == null || player2 == null)
            throw new Exception("Players not found!");

        var game = new Game
        {
            Player1Id = gameDto.Player1Id,
            Player2Id = gameDto.Player2Id,
            Player1 = player1,
            Player2 = player2,
            GameStatus = "Ongoing",
            CreatedAt = DateTime.UtcNow
        };

        await _gameRepository.AddAsync(game);
        return game;
    }

    public async Task<Game?> GetGameByIdAsync(int gameId)
    {
        return await _gameRepository.GetByIdAsync(gameId);
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        return await _gameRepository.GetAllAsync();
    }

   public async Task<bool> UpdateGameAsync(int gameId, UpdateGameDto dto)
{
    var game = await _gameRepository.GetByIdAsync(gameId);
    if (game == null) return false;

    game.GameStatus = dto.GameStatus;

    // Ensure WinnerId is not null or empty
    if (!string.IsNullOrEmpty(dto.WinnerId))
    {
        var winner = await _userRepository.GetByIdAsync(dto.WinnerId);
        if (winner != null && (winner.Id == game.Player1.Id || winner.Id == game.Player2.Id))
        {
            game.Winner = winner;
            game.WinnerId = winner.Id; // Ensures WinnerId is stored in the database

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
        }
    }

    await _gameRepository.UpdateAsync(game);
    return true;
}


    public async Task<bool> DeleteGameAsync(int gameId)
    {
        return await _gameRepository.DeleteAsync(gameId);
    }
}
