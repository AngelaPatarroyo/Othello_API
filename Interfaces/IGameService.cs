using Othello_API.DTOs;



public interface IGameService
{
    Task<Game> CreateGameAsync(StartGameDto gameDto);
    Task<Game?> GetGameByIdAsync(int gameId);
    Task<List<Game>> GetAllGamesAsync();
    Task<bool> UpdateGameAsync(int gameId, UpdateGameDto dto);
    Task<bool> DeleteGameAsync(int gameId);
}
