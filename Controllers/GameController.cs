using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Othello_API.DTOs;
using Othello_API.Services;
using Othello_API.Models;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ApplicationDbContext _context;

    public GameController(IGameService gameService, ApplicationDbContext context)
    {
        _gameService = gameService;
        _context = context;
    }

    // Start a new game
    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromBody] StartGameDto gameDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var game = await _gameService.CreateGameAsync(gameDto);
        if (game == null)
            return BadRequest("Game could not be created.");

        // Ensure Player1 and Player2 are fully loaded
        game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .FirstOrDefaultAsync(g => g.GameId == game.GameId);

        if (game?.Player1 == null || game?.Player2 == null)
            return BadRequest("Failed to associate players with the game.");

        var response = new
        {
            gameId = game.GameId,
            gameStatus = game.GameStatus,
            createdAt = game.CreatedAt,
            player1 = new
            {
                id = game.Player1Id,
                userName = game.Player1?.UserName,
                email = game.Player1?.Email
            },
            player2 = new
            {
                id = game.Player2Id,
                userName = game.Player2?.UserName,
                email = game.Player2?.Email
            }
        };

        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, response);
    }

    // Get a single game by gameId
    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetGame(int gameId)
    {
        var game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
            return NotFound("Game not found.");

        var response = new
        {
            gameId = game.GameId,
            gameStatus = game.GameStatus,
            createdAt = game.CreatedAt,
            player1 = new
            {
                id = game.Player1Id,
                userName = game.Player1?.UserName,
                email = game.Player1?.Email
            },
            player2 = new
            {
                id = game.Player2Id,
                userName = game.Player2?.UserName,
                email = game.Player2?.Email
            },
            winner = game.Winner != null ? new
            {
                id = game.Winner.Id,
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
        var games = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .ToListAsync();

        var response = games.Select(game => new
        {
            gameId = game.GameId,
            gameStatus = game.GameStatus,
            createdAt = game.CreatedAt,
            player1 = new
            {
                id = game.Player1Id,
                userName = game.Player1?.UserName,
                email = game.Player1?.Email
            },
            player2 = new
            {
                id = game.Player2Id,
                userName = game.Player2?.UserName,
                email = game.Player2?.Email
            },
            winner = game.Winner != null ? new
            {
                id = game.Winner.Id,
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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _gameService.UpdateGameAsync(gameId, dto);
        if (!success)
            return NotFound("Game not found.");

        return Ok("Game updated successfully.");
    }

    // Delete a game
    [HttpDelete("{gameId}")]
    public async Task<IActionResult> DeleteGame(int gameId)
    {
        var success = await _gameService.DeleteGameAsync(gameId);
        if (!success)
            return NotFound("Game not found.");

        return NoContent();
    }
}
