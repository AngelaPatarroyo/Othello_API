using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Othello_API.DTOs;
using Othello_API.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;


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

    [HttpPost("start")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Start a new game", Description = "Creates a new game between two players and returns game details.")]
    [SwaggerResponse(201, "Game successfully created", typeof(GameDto))]
    [SwaggerResponse(400, "Invalid game request")]
    public async Task<IActionResult> StartGame([FromBody] StartGameDto gameDto)
    {
        _logger.LogInformation("User is starting a new game with Player1: {Player1Id}, Player2: {Player2Id}.", gameDto.Player1Id, gameDto.Player2Id);

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

        if (game == null || game.Player1 == null || game.Player2 == null)
        {
            _logger.LogError("Failed to associate players with the game.");
            return BadRequest("Failed to associate players with the game.");
        }

        _logger.LogInformation("Game {GameId} successfully started.", game.GameId);

        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, new GameDto(game));
    }

    [HttpPost("challenge")]
    [Authorize]
    [SwaggerOperation(Summary = "Challenge another user to a game", Description = "Authenticated user challenges another player to a game.")]
    [SwaggerResponse(201, "Game successfully created", typeof(GameDto))]
    [SwaggerResponse(400, "Invalid challenge request")]
    public async Task<IActionResult> ChallengePlayer([FromBody] ChallengeRequestDto request)
    {
        var challengerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (challengerId == null)
        {
            return Unauthorized();
        }

        if (challengerId == request.OpponentId)
        {
            return BadRequest("You cannot challenge yourself.");
        }

        var game = await _gameService.CreateGameAsync(new StartGameDto
        {
            Player1Id = challengerId,
            Player2Id = request.OpponentId
        });

        if (game == null)
        {
            return BadRequest("Failed to create game.");
        }

        game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .FirstOrDefaultAsync(g => g.GameId == game.GameId);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        return CreatedAtAction(nameof(GetGame), new { gameId = game.GameId }, new GameDto(game));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    [HttpGet("{gameId}")]
    [SwaggerOperation(Summary = "Get game by ID", Description = "Fetches details of a specific game.")]
    [SwaggerResponse(200, "Successfully retrieved game", typeof(GameDto))]
    [SwaggerResponse(404, "Game not found")]
    public async Task<IActionResult> GetGame(int gameId)
    {
        _logger.LogInformation("Retrieving details for GameId: {GameId}.", gameId);

        var game = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found.", gameId);
            return NotFound(new { message = "Game not found." });
        }

        _logger.LogInformation("Game {GameId} retrieved successfully.", gameId);

        return Ok(new GameDto(game));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Get all games (Admin only)", Description = "Fetches all games. Requires Admin role.")]
    [SwaggerResponse(200, "Successfully retrieved list of games", typeof(List<GameDto>))]
    public async Task<IActionResult> GetAllGames()
    {
        _logger.LogInformation("Admin is fetching all games.");

        var games = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return Ok(games.Select(game => new GameDto(game)).ToList());
    }

    [HttpPut("{gameId}")]
    [SwaggerOperation(Summary = "Update game", Description = "Updates the status and winner of an existing game.")]
    [SwaggerResponse(200, "Game updated successfully", typeof(GameDto))]
    [SwaggerResponse(404, "Game not found")]
    [SwaggerResponse(400, "Invalid game update request")]
    public async Task<IActionResult> UpdateGame(int gameId, [FromBody] UpdateGameDto updateGameDto)
    {
        _logger.LogInformation("Attempting to update game with ID {GameId}.", gameId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid update request.");
            return BadRequest(ModelState);
        }

        var success = await _gameService.UpdateGameAsync(gameId, updateGameDto);
        if (!success)
        {
            _logger.LogWarning("Game with ID {GameId} not found or update failed.", gameId);
            return NotFound(new { message = "Game not found or update failed." });
        }

        var updatedGame = await _context.Games
            .Include(g => g.Player1)
            .Include(g => g.Player2)
            .Include(g => g.Winner)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        return Ok(new GameDto(updatedGame!));
    }

    [HttpDelete("{gameId}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete a game (Admin only)", Description = "Deletes a game by ID. Requires Admin role.")]
    [SwaggerResponse(204, "Game deleted successfully")]
    [SwaggerResponse(404, "Game not found")]
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
