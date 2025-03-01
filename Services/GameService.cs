using Othello_API.DTOs;
using Othello_API.Models;
using Othello_API.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;

    public GameService(IGameRepository gameRepository, IUserRepository userRepository, ApplicationDbContext context)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _context = context;
    }

  
    public async Task<Game> CreateGameAsync(StartGameDto gameDto)
    {
        // Validate input
        if (gameDto == null || string.IsNullOrEmpty(gameDto.Player1Id) || string.IsNullOrEmpty(gameDto.Player2Id))
        {
            throw new ArgumentException("Player1Id and Player2Id are required.");
        }

        // Check if players exist
        var player1 = await _userRepository.GetByIdAsync(gameDto.Player1Id);
        var player2 = await _userRepository.GetByIdAsync(gameDto.Player2Id);

        if (player1 == null || player2 == null)
        {
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

        // Load Player1 and Player2 to ensure they are properly assigned
        game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .FirstOrDefaultAsync(g => g.GameId == game.GameId);

        if (game?.Player1 == null || game?.Player2 == null)
        {
            throw new Exception("Failed to associate players with the game.");
        }

        return game;
    }

    /// <summary>
    /// Retrieves a game by its ID.
    /// </summary>
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

        if (!string.IsNullOrEmpty(dto.WinnerId))
        {
            var winner = await _userRepository.GetByIdAsync(dto.WinnerId);
            if (winner != null && (winner.Id == game.Player1Id || winner.Id == game.Player2Id))
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
