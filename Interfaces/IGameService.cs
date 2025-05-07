using Othello_API.DTOs;
using Othello_API.Models;




public interface IGameService
{
    Task<Game> CreateGameAsync(StartGameDto gameDto);
    Task<Game?> GetGameByIdAsync(int gameId);
    Task<List<Game>> GetAllGamesAsync();
    Task<bool> UpdateGameAsync(int gameId, UpdateGameDto dto);
    Task<bool> DeleteGameAsync(int gameId);
    void GetGameById(int gameId);
}
