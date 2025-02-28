using Microsoft.AspNetCore.Mvc;
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

    // Start a new game
    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromBody] StartGameDto gameDto)
    {
        var game = await _gameService.CreateGameAsync(gameDto);
        
        if (game == null) 
            return BadRequest("Game could not be created");

        // Custom response to keep gameId and rename UserId to id
        var response = new
        {
            gameId = game.GameId, // Keep gameId
            gameStatus = game.GameStatus,
            createdAt = game.CreatedAt,
            player1 = new
            {
                id = game.Player1.Id, // Keep userId as id
                userName = game.Player1.UserName,
                email = game.Player1.Email
            },
            player2 = new
            {
                id = game.Player2.Id, // Keep userId as id
                userName = game.Player2.UserName,
                email = game.Player2.Email
            }
        };

        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, response);
    }

    // Get a single game by gameId
    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetGame(int gameId)
    {
        var game = await _gameService.GetGameByIdAsync(gameId);
        if (game == null) return NotFound("Game not found.");

        var response = new
        {
            gameId = game.GameId, // Keep gameId
            gameStatus = game.GameStatus,
            createdAt = game.CreatedAt,
            player1 = new
            {
                id = game.Player1.Id, // Keep userId as id
                userName = game.Player1.UserName,
                email = game.Player1.Email
            },
            player2 = new
            {
                id = game.Player2.Id, // Keep userId as id
                userName = game.Player2.UserName,
                email = game.Player2.Email
            },
            winner = game.Winner != null ? new
            {
                id = game.Winner.Id, // Keep userId as id
                userName = game.Winner.UserName,
                email = game.Winner.Email
            } : null
        };

        return Ok(response);
    }

    // Get all games
    [HttpGet]
    public async Task<IActionResult> GetAllGames()
    {
        var games = await _gameService.GetAllGamesAsync();

        var response = games.Select(game => new
        {
            gameId = game.GameId, // Keep gameId
            gameStatus = game.GameStatus,
            createdAt = game.CreatedAt,
            player1 = new
            {
                id = game.Player1.Id, // Keep userId as id
                userName = game.Player1.UserName,
                email = game.Player1.Email
            },
            player2 = new
            {
                id = game.Player2.Id, // Keep userId as id
                userName = game.Player2.UserName,
                email = game.Player2.Email
            },
            winner = game.Winner != null ? new
            {
                id = game.Winner.Id, // Keep userId as id
                userName = game.Winner.UserName,
                email = game.Winner.Email
            } : null
        });

        return Ok(response);
    }

    // Update game details
    [HttpPut("{gameId}")]
    public async Task<IActionResult> UpdateGame(int gameId, [FromBody] UpdateGameDto dto)
    {
        var success = await _gameService.UpdateGameAsync(gameId, dto);
        if (!success) return NotFound("Game not found.");

        return Ok("Game updated successfully.");
    }

    // Delete a game
    [HttpDelete("{gameId}")]
    public async Task<IActionResult> DeleteGame(int gameId)
    {
        var success = await _gameService.DeleteGameAsync(gameId);
        if (!success) return NotFound("Game not found.");

        return Ok("Game deleted successfully.");
    }
}
