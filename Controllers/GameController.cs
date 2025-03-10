using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Othello_API.DTOs;
using Othello_API.Models;
using Swashbuckle.AspNetCore.Annotations;

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

    /// <summary>
    /// Starts a new game between two players.
    /// </summary>
    /// <param name="gameDto">The game details including Player IDs.</param>
    /// <returns>Returns the created game details.</returns>
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

    /// <summary>
    /// Retrieves a game by its ID.
    /// </summary>
    /// <param name="gameId">The ID of the game to retrieve.</param>
    /// <returns>Returns the game details.</returns>
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

    /// <summary>
    /// Retrieves all games. (Admin only)
    /// </summary>
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

    /// <summary>
    /// Updates the status of an existing game.
    /// </summary>
    /// <param name="gameId">The ID of the game to update.</param>
    /// <param name="updateGameDto">The game details to update.</param>
    /// <returns>Returns the updated game details.</returns>
    [HttpPut("{gameId}")]
    [SwaggerOperation(Summary = "Update game", Description = "Updates the status and winner of an existing game.")]
    [SwaggerResponse(200, "Game updated successfully", typeof(GameDto))]
    [SwaggerResponse(404, "Game not found")]
    [SwaggerResponse(400, "Invalid game update request")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> UpdateGame(int gameId, [FromBody] UpdateGameDto updateGameDto)
    {
        _logger.LogInformation("Attempting to update game with ID {GameId}.", gameId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid update request.");
            return BadRequest(ModelState);
        }

        var game = await _context.Games.FindAsync(gameId);

        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found.", gameId);
            return NotFound(new { message = "Game not found." });
        }

        // Update game status and winner
        game.GameStatus = updateGameDto.GameStatus;
        game.WinnerId = updateGameDto.WinnerId;

        _context.Games.Update(game);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Game with ID {GameId} updated successfully.", gameId);
        return Ok(new GameDto(game)); // Return updated game details
    }

    /// <summary>
    /// Deletes a game. (Admin only)
    /// </summary>
    /// <param name="gameId">The ID of the game to delete.</param>
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
