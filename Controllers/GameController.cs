using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Othello_API.DTOs;
using Othello_API.Interfaces;
using Othello_API.Models;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] //  Restrict all endpoints to Admin users
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameService gameService, ApplicationDbContext context, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _context = context;
        _logger = logger;
    }

    // Start a new game
    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromBody] StartGameDto gameDto)
    {
        _logger.LogInformation("Admin is starting a new game with Player1: {Player1Id}, Player2: {Player2Id}.", gameDto.Player1Id, gameDto.Player2Id);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid game start request.");
            return BadRequest(ModelState);
        }

        var game = await _gameService.CreateGameAsync(gameDto);
        if (game == null)
        {
            _logger.LogError("Game creation failed.");
            return BadRequest("Game could not be created.");
        }

        game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .FirstOrDefaultAsync(g => g.GameId == game.GameId);

        if (game?.Player1 == null || game?.Player2 == null)
        {
            _logger.LogError("Failed to associate players with the game.");
            return BadRequest("Failed to associate players with the game.");
        }

        _logger.LogInformation("Game {GameId} successfully started.", game.GameId);

        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, new
        {
            game.GameId,
            game.GameStatus,
            game.CreatedAt,
            Player1 = new { game.Player1Id, game.Player1?.UserName, game.Player1?.Email },
            Player2 = new { game.Player2Id, game.Player2?.UserName, game.Player2?.Email }
        });
    }

    // Get a single game by gameId
    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetGame(int gameId)
    {
        _logger.LogInformation("Admin is retrieving details for GameId: {GameId}.", gameId);

        var game = await _context.Games
            .Include(g => g.Player1)  // Ensure Player1 is loaded
            .Include(g => g.Player2)  // Ensure Player2 is loaded
            .Include(g => g.Winner)   // Ensure Winner is loaded
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found.", gameId);
            return NotFound(new { message = "Game not found." });
        }

        _logger.LogInformation("Game {GameId} retrieved successfully.", gameId);

        return Ok(new
        {
            game.GameId,
            game.GameStatus,
            game.CreatedAt,
            Player1 = game.Player1 != null ? new
            {
                id = game.Player1Id,
                userName = game.Player1.UserName ?? "Unknown",
                email = game.Player1.Email ?? "No Email"
            } : null,

            Player2 = game.Player2 != null ? new
            {
                id = game.Player2Id,
                userName = game.Player2.UserName ?? "Unknown",
                email = game.Player2.Email ?? "No Email"
            } : null,

            Winner = game.Winner != null ? new
            {
                id = game.Winner.Id,
                userName = game.Winner.UserName ?? "Unknown",
                email = game.Winner.Email ?? "No Email"
            } : null
        });
    }


    // Get all games
    [HttpGet]
    public async Task<IActionResult> GetAllGames([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Fetching all games with pagination (Page: {Page}, PageSize: {PageSize})", page, pageSize);

        // Ensure page size is reasonable (prevent abuse)
        pageSize = Math.Clamp(pageSize, 1, 50); // Limit page size between 1 and 50

        var query = _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner);

        var totalGames = await query.CountAsync(); // Get total number of games

        var games = await query
            .OrderByDescending(g => g.CreatedAt) // Sort by newest games first
            .Skip((page - 1) * pageSize) // Skip previous pages
            .Take(pageSize) // Limit results to pageSize
            .ToListAsync();

        return Ok(new
        {
            totalGames,
            totalPages = (int)Math.Ceiling((double)totalGames / pageSize),
            currentPage = page,
            pageSize,
            games = games.Select(game => new
            {
                gameId = game.GameId,
                gameStatus = game.GameStatus,
                createdAt = game.CreatedAt,
                player1 = new { id = game.Player1Id, userName = game.Player1?.UserName },
                player2 = new { id = game.Player2Id, userName = game.Player2?.UserName },
                winner = game.Winner != null ? new { id = game.Winner.Id, userName = game.Winner.UserName } : null
            })
        });
    }

    // Update game details
    [HttpPut("{gameId}")]
    public async Task<IActionResult> UpdateGame(int gameId, [FromBody] UpdateGameDto dto)
    {
        _logger.LogInformation("Admin is updating Game {GameId}.", gameId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid update request for Game {GameId}.", gameId);
            return BadRequest(ModelState);
        }

        var success = await _gameService.UpdateGameAsync(gameId, dto);
        if (!success)
        {
            _logger.LogWarning("Game {GameId} not found for update.", gameId);
            return NotFound("Game not found.");
        }

        _logger.LogInformation("Game {GameId} updated successfully.", gameId);
        return Ok("Game updated successfully.");
    }

    // Delete a game
    [HttpDelete("{gameId}")]
    public async Task<IActionResult> DeleteGame(int gameId)
    {
        _logger.LogInformation("Admin is deleting Game {GameId}.", gameId);

        var success = await _gameService.DeleteGameAsync(gameId);
        if (!success)
        {
            _logger.LogWarning("Game {GameId} not found for deletion.", gameId);
            return NotFound("Game not found.");
        }

        _logger.LogInformation("Game {GameId} deleted successfully.", gameId);
        return NoContent();
    }
}
