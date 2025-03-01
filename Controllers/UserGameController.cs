using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Othello_API.Models;
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
            .Include(ug => ug.User)
            .Include(ug => ug.Game)
            .Select(ug => new
            {
                ug.UserGameId,
                ug.UserId,
                UserName = ug.User != null ? ug.User.UserName : "Unknown", // Avoid null errors
                ug.GameId,
                GameStatus = ug.Game != null ? ug.Game.GameStatus : "Unknown", // Avoid null errors
                ug.TotalWins,
                ug.TotalLosses,
                ug.TotalGames
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
            .Include(ug => ug.User)
            .Include(ug => ug.Game)
            .FirstOrDefaultAsync(ug => ug.UserGameId == id);

        if (userGame == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            userGame.UserGameId,
            userGame.UserId,
            UserName = userGame.User?.UserName ?? "Unknown",
            userGame.GameId,
            GameStatus = userGame.Game?.GameStatus ?? "Unknown",
            userGame.TotalWins,
            userGame.TotalLosses,
            userGame.TotalGames
        });
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
    var user = await _context.Users.FindAsync(userGameDto.UserId);
    if (user == null)
    {
        return BadRequest("User does not exist.");
    }

    // Validate if the Game exists and load it fully
    var game = await _context.Games
        .Include(g => g.Player1)
        .Include(g => g.Player2)
        .FirstOrDefaultAsync(g => g.GameId == userGameDto.GameId);

    if (game == null)
    {
        return BadRequest("Game does not exist.");
    }

    // Check if the User is one of the Players
    if (game.Player1Id != userGameDto.UserId && game.Player2Id != userGameDto.UserId)
    {
        return BadRequest("User is not a participant in this game.");
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
            return NotFound();
        }

        _context.UserGames.Remove(userGame);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
