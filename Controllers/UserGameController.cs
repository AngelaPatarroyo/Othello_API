using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Othello_API.Models;
using Othello_API.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UserGameController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserGameController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/UserGame
    [HttpGet]
    [Produces("application/json")]
    public async Task<IActionResult> GetUserGames()
    {
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
            return NotFound("No user games found.");
        }

        return Ok(userGames);
    }

    // GET: api/UserGame/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserGameById(int id)
    {
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
            return NotFound("User game not found.");
        }

        return Ok(userGame);
    }

    // POST: api/UserGame
    [HttpPost]
    public async Task<IActionResult> CreateUserGame([FromBody] UserGameDto userGameDto)
    {
        if (userGameDto == null || string.IsNullOrEmpty(userGameDto.UserId) || userGameDto.GameId == 0)
        {
            return BadRequest("UserId and GameId are required.");
        }

        // Validate if the User exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == userGameDto.UserId);
        if (!userExists)
        {
            return BadRequest("User does not exist.");
        }

        // Validate if the Game exists
        var gameExists = await _context.Games.AnyAsync(g => g.GameId == userGameDto.GameId);
        if (!gameExists)
        {
            return BadRequest("Game does not exist.");
        }

        // Check if the user is already associated with the game
        var existingUserGame = await _context.UserGames
            .FirstOrDefaultAsync(ug => ug.UserId == userGameDto.UserId && ug.GameId == userGameDto.GameId);

        if (existingUserGame != null)
        {
            return Conflict("User is already part of this game.");
        }

        // Create the UserGame entry
        var newUserGame = new UserGame
        {
            UserId = userGameDto.UserId,
            GameId = userGameDto.GameId,
            TotalWins = userGameDto.TotalWins ?? 0,
            TotalLosses = userGameDto.TotalLosses ?? 0,
            TotalGames = userGameDto.TotalGames ?? 0
        };

        _context.UserGames.Add(newUserGame);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserGameById), new { id = newUserGame.UserGameId }, newUserGame);
    }

    // DELETE: api/UserGame/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserGame(int id)
    {
        var userGame = await _context.UserGames.FindAsync(id);
        if (userGame == null)
        {
            return NotFound("User game not found.");
        }

        _context.UserGames.Remove(userGame);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
