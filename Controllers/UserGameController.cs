using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Othello_API.DTOs;
using Othello_API.Models;

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

    // GET: api/UserGame
    [HttpGet]
    [Produces("application/json")]
    public async Task<IActionResult> GetUserGames()
    {
        _logger.LogInformation("Fetching all user-game relationships.");

        var userGames = await _context.UserGames
            .Select(ug => new
            {
                ug.UserGameId,
                ug.UserId,
                ug.GameId
            })
            .ToListAsync();

        if (!userGames.Any())
        {
            _logger.LogWarning("No user-game relationships found.");
            return NotFound("No user games found.");
        }

        _logger.LogInformation("Successfully retrieved {UserGameCount} user-game relationships.", userGames.Count);
        return Ok(userGames);
    }

    // GET: api/UserGame/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserGameById(int id)
    {
        _logger.LogInformation("Fetching user-game relationship with ID {UserGameId}.", id);

        var userGame = await _context.UserGames
            .Where(ug => ug.UserGameId == id)
            .Select(ug => new
            {
                ug.UserGameId,
                ug.UserId,
                ug.GameId
            })
            .FirstOrDefaultAsync();

        if (userGame == null)
        {
            _logger.LogWarning("UserGame with ID {UserGameId} not found.", id);
            return NotFound("User game not found.");
        }

        _logger.LogInformation("Successfully retrieved user-game relationship with ID {UserGameId}.", id);
        return Ok(userGame);
    }

    // POST: api/UserGame
    [HttpPost]
    public async Task<IActionResult> CreateUserGame([FromBody] UserGameDto userGameDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid UserGameDto request received.");
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Attempting to create a new UserGame for UserId: {UserId}, GameId: {GameId}.", userGameDto.UserId, userGameDto.GameId);

        // Validate if the User exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == userGameDto.UserId);
        if (!userExists)
        {
            _logger.LogWarning("User with ID {UserId} does not exist.", userGameDto.UserId);
            return BadRequest("User does not exist.");
        }

        // Validate if the Game exists
        var gameExists = await _context.Games.AnyAsync(g => g.GameId == userGameDto.GameId);
        if (!gameExists)
        {
            _logger.LogWarning("Game with ID {GameId} does not exist.", userGameDto.GameId);
            return BadRequest("Game does not exist.");
        }

        // Check if the user is already associated with the game
        var existingUserGame = await _context.UserGames
            .FirstOrDefaultAsync(ug => ug.UserId == userGameDto.UserId && ug.GameId == userGameDto.GameId);

        if (existingUserGame != null)
        {
            _logger.LogWarning("UserId {UserId} is already part of the game with GameId {GameId}.", userGameDto.UserId, userGameDto.GameId);
            return Conflict("User is already part of this game.");
        }

        // Create the UserGame entry
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
        return CreatedAtAction(nameof(GetUserGameById), new { id = newUserGame.UserGameId }, newUserGame);
    }

    // DELETE: api/UserGame/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserGame(int id)
    {
        _logger.LogInformation("Attempting to delete UserGame with ID {UserGameId}.", id);

        var userGame = await _context.UserGames.FindAsync(id);
        if (userGame == null)
        {
            _logger.LogWarning("UserGame with ID {UserGameId} not found for deletion.", id);
            return NotFound("User game not found.");
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
            return StatusCode(500, "An internal server error occurred.");
        }
    }
}
