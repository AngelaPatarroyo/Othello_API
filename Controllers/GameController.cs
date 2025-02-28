using Microsoft.AspNetCore.Mvc;
using Othello_API.Models;
using Othello_API.Dtos;
using Othello_API.Services;
using System.Threading.Tasks;

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
        
        if (game == null) 
            return BadRequest("Game could not be created");

        // Remove sensitive player data before returning the response
        var response = new
        {
            game.GameId,
            game.GameStatus,
            game.CreatedAt,
            Player1 = new
            {
                game.Player1.UserName,
                game.Player1.Email
            },
            Player2 = new
            {
                game.Player2.UserName,
                game.Player2.Email
            }
        };

        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, response);
    }

    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetGame(int gameId)
    {
        var game = await _gameService.GetGameByIdAsync(gameId);
        if (game == null) return NotFound();

        var response = new
        {
            game.GameId,
            game.GameStatus,
            game.CreatedAt,
            Player1 = new
            {
                game.Player1.UserName,
                game.Player1.Email
            },
            Player2 = new
            {
                game.Player2.UserName,
                game.Player2.Email
            }
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGames()
    {
        var games = await _gameService.GetAllGamesAsync();
        
        var response = games.Select(game => new
        {
            game.GameId,
            game.GameStatus,
            game.CreatedAt,
            Player1 = new
            {
                game.Player1.UserName,
                game.Player1.Email
            },
            Player2 = new
            {
                game.Player2.UserName,
                game.Player2.Email
            }
        });

        return Ok(response);
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
