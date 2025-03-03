using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Othello_API.DTOs;
using Othello_API.Models;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/[controller]")]
[ApiController]
public class UserGameController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserGameController> _logger;

    public UserGameController(ApplicationDbContext context, ILogger<UserGameController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all user-game relationships. (Admin only)
    /// </summary>
    /// <returns>Returns a list of user-game associations.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")] // Restrict full leaderboard access to admins only
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Get all user-game relationships (Admin only)", Description = "Fetches all user-game relationships. Requires Admin role.")]
    [SwaggerResponse(200, "Successfully retrieved user-game relationships", typeof(List<UserGameDto>))]
    [SwaggerResponse(404, "No user-game relationships found")]
    public async Task<IActionResult> GetUserGames()
    {
        _logger.LogInformation("Fetching all user-game relationships.");

        var userGames = await _context.UserGames
            .Select(ug => new UserGameDto
            {
                UserGameId = ug.UserGameId,
                UserId = ug.UserId,
                GameId = ug.GameId,
                TotalWins = ug.TotalWins,
                TotalLosses = ug.TotalLosses,
                TotalGames = ug.TotalGames
            })
            .ToListAsync();

        if (!userGames.Any())
        {
            _logger.LogWarning("No user-game relationships found.");
            return NotFound(new { message = "No user games found." });
        }

        _logger.LogInformation("Successfully retrieved {UserGameCount} user-game relationships.", userGames.Count);
        return Ok(userGames);
    }

    /// <summary>
    /// Retrieves a user-game relationship by ID.
    /// </summary>
    /// <param name="id">The ID of the user-game relationship.</param>
    /// <returns>Returns the user-game relationship details.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get user-game by ID", Description = "Fetches a user-game relationship by ID.")]
    [SwaggerResponse(200, "Successfully retrieved user-game relationship", typeof(UserGameDto))]
    [SwaggerResponse(404, "User-game relationship not found")]
    public async Task<IActionResult> GetUserGameById(int id)
    {
        _logger.LogInformation("Fetching user-game relationship with ID {UserGameId}.", id);

        var userGame = await _context.UserGames
            .Where(ug => ug.UserGameId == id)
            .Select(ug => new UserGameDto
            {
                UserGameId = ug.UserGameId,
                UserId = ug.UserId,
                GameId = ug.GameId,
                TotalWins = ug.TotalWins,
                TotalLosses = ug.TotalLosses,
                TotalGames = ug.TotalGames
            })
            .FirstOrDefaultAsync();

        if (userGame == null)
        {
            _logger.LogWarning("UserGame with ID {UserGameId} not found.", id);
            return NotFound(new { message = "User game not found." });
        }

        _logger.LogInformation("Successfully retrieved user-game relationship with ID {UserGameId}.", id);
        return Ok(userGame);
    }

    /// <summary>
    /// Creates a new user-game relationship.
    /// </summary>
    /// <param name="userGameDto">The user-game details.</param>
    /// <returns>Returns the created user-game relationship.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Create user-game relationship", Description = "Associates a user with a game.")]
    [SwaggerResponse(201, "User-game relationship created successfully", typeof(UserGameDto))]
    [SwaggerResponse(400, "Invalid user-game request")]
    [SwaggerResponse(409, "User is already part of this game")]
    public async Task<IActionResult> CreateUserGame([FromBody] UserGameDto userGameDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid UserGameDto request received.");
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Attempting to create a new UserGame for UserId: {UserId}, GameId: {GameId}.", userGameDto.UserId, userGameDto.GameId);

        var userExists = await _context.Users.AnyAsync(u => u.Id == userGameDto.UserId);
        if (!userExists)
        {
            _logger.LogWarning("User with ID {UserId} does not exist.", userGameDto.UserId);
            return BadRequest(new { message = "User does not exist." });
        }

        var gameExists = await _context.Games.AnyAsync(g => g.GameId == userGameDto.GameId);
        if (!gameExists)
        {
            _logger.LogWarning("Game with ID {GameId} does not exist.", userGameDto.GameId);
            return BadRequest(new { message = "Game does not exist." });
        }

        var existingUserGame = await _context.UserGames
            .FirstOrDefaultAsync(ug => ug.UserId == userGameDto.UserId && ug.GameId == userGameDto.GameId);

        if (existingUserGame != null)
        {
            _logger.LogWarning("UserId {UserId} is already part of the game with GameId {GameId}.", userGameDto.UserId, userGameDto.GameId);
            return Conflict(new { message = "User is already part of this game." });
        }

        var newUserGame = new UserGame
        {
            UserId = userGameDto.UserId,
            GameId = userGameDto.GameId,
            TotalWins = userGameDto.TotalWins,
            TotalLosses = userGameDto.TotalLosses,
            TotalGames = userGameDto.TotalGames
        };

        _context.UserGames.Add(newUserGame);
        await _context.SaveChangesAsync();

        _logger.LogInformation("UserGame created successfully with ID {UserGameId}.", newUserGame.UserGameId);
        return CreatedAtAction(nameof(GetUserGameById), new { id = newUserGame.UserGameId }, new UserGameDto
        {
            UserGameId = newUserGame.UserGameId,
            UserId = newUserGame.UserId,
            GameId = newUserGame.GameId,
            TotalWins = newUserGame.TotalWins,
            TotalLosses = newUserGame.TotalLosses,
            TotalGames = newUserGame.TotalGames
        });
    }

    /// <summary>
    /// Deletes a user-game relationship. (Admin only)
    /// </summary>
    /// <param name="id">The ID of the user-game relationship to delete.</param>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete user-game relationship (Admin only)", Description = "Deletes a user-game relationship by ID.")]
    [SwaggerResponse(204, "User-game relationship deleted successfully")]
    [SwaggerResponse(404, "User-game relationship not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> DeleteUserGame(int id)
    {
        _logger.LogInformation("Admin attempting to delete UserGame with ID {UserGameId}.", id);

        var userGame = await _context.UserGames.FindAsync(id);
        if (userGame == null)
        {
            _logger.LogWarning("UserGame with ID {UserGameId} not found for deletion.", id);
            return NotFound(new { message = "User game not found." });
        }

        try
        {
            _context.UserGames.Remove(userGame);
            await _context.SaveChangesAsync();

            _logger.LogInformation("UserGame with ID {UserGameId} deleted successfully.", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting UserGame with ID {UserGameId}.", id);
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }
}
