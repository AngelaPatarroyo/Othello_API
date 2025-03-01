using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Othello_API.DTOs;


[Route("api/[controller]")]
[ApiController]
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
        _logger.LogInformation("Received request to start a new game with Player1: {Player1Id}, Player2: {Player2Id}.", gameDto.Player1Id, gameDto.Player2Id);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid game start request.");
            return BadRequest(ModelState);
        }

        var game = await _gameService.CreateGameAsync(gameDto);
        if (game == null)
        {
            _logger.LogError("Failed to create a new game.");
            return BadRequest("Game could not be created.");
        }

        // Ensure Player1 and Player2 are fully loaded
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
        _logger.LogInformation("Fetching game details for GameId: {GameId}.", gameId);

        var game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found.", gameId);
            return NotFound("Game not found.");
        }

        _logger.LogInformation("Game {GameId} retrieved successfully.", gameId);

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
        _logger.LogInformation("Fetching all games.");

        var games = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .ToListAsync();

        if (games.Count == 0)
        {
            _logger.LogWarning("No games found in the database.");
            return NotFound("No games available.");
        }

        _logger.LogInformation("Successfully retrieved {Count} games.", games.Count);

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
        _logger.LogInformation("Received request to update Game {GameId}.", gameId);

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
        _logger.LogInformation("Received request to delete Game {GameId}.", gameId);

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
