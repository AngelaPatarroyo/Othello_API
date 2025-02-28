using Microsoft.AspNetCore.Mvc;
using Othello_API.Models;
using Othello_API.Dtos;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromBody] StartGameDto gameDto)
    {
        var game = await _gameService.CreateGameAsync(gameDto);
        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, game);
    }

    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetGame(int gameId)
    {
        var game = await _gameService.GetGameByIdAsync(gameId);
        if (game == null) return NotFound();
        return Ok(game);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGames()
    {
        var games = await _gameService.GetAllGamesAsync();
        return Ok(games);
    }

    [HttpPut("{gameId}")]
    public async Task<IActionResult> UpdateGame(int gameId, [FromBody] UpdateGameDto dto)
    {
        var success = await _gameService.UpdateGameAsync(gameId, dto);
        if (!success) return NotFound("Game not found");

        return Ok("Game updated successfully");
    }

    [HttpDelete("{gameId}")]
    public async Task<IActionResult> DeleteGame(int gameId)
    {
        var success = await _gameService.DeleteGameAsync(gameId);
        if (!success) return NotFound("Game not found");

        return Ok("Game deleted successfully");
    }
}
