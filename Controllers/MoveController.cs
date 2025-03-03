using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Othello_API.Interfaces;
using Othello_API.Dtos;
using Swashbuckle.AspNetCore.Annotations;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Requires authentication
public class MoveController : ControllerBase
{
    private readonly IMoveService _moveService;
    private readonly ILogger<MoveController> _logger;

    public MoveController(IMoveService moveService, ILogger<MoveController> logger)
    {
        _moveService = moveService;
        _logger = logger;
    }

    /// <summary>
    /// Makes a move in the specified game.
    /// </summary>
    /// <param name="gameId">The ID of the game where the move is made.</param>
    /// <param name="moveDto">The move details including row, column, and player ID.</param>
    /// <returns>Returns the created move details.</returns>
    [Authorize]
    [HttpPost("{gameId}/move")]
    [SwaggerOperation(Summary = "Make a move", Description = "Records a move in the specified game.")]
    [SwaggerResponse(200, "Move successfully recorded", typeof(MoveDto))]
    [SwaggerResponse(404, "Game not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> MakeMove(int gameId, [FromBody] MoveDto moveDto)
    {
        _logger.LogInformation("Attempting to make a move for game {GameId} by player {PlayerId}.", gameId, moveDto.PlayerId);

        try
        {
            var move = await _moveService.MakeMoveAsync(gameId, moveDto);
            if (move == null)
            {
                _logger.LogWarning("Game {GameId} not found when trying to make a move.", gameId);
                return NotFound(new { message = "Game not found." });
            }

            var response = new MoveDto
            {
                MoveId = move.MoveId,
                GameId = move.GameId,
                PlayerId = move.PlayerId,
                Row = move.Row,
                Column = move.Column,
                MoveNumber = move.MoveNumber
            };

            _logger.LogInformation("Move successfully made for game {GameId}, MoveId {MoveId}.", gameId, move.MoveId);
            return Ok(new { message = "Move recorded successfully", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while making a move for game {GameId}: {ErrorMessage}", gameId, ex.Message);
            return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all moves for a specific game.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <returns>Returns a list of moves for the game.</returns>
    [HttpGet("{gameId}/moves")]
    [SwaggerOperation(Summary = "Get all moves for a game", Description = "Fetches all moves made in the specified game.")]
    [SwaggerResponse(200, "Moves retrieved successfully", typeof(List<MoveDto>))]
    [SwaggerResponse(404, "No moves found for this game")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> GetMoves(int gameId)
    {
        _logger.LogInformation("Fetching all moves for game {GameId}.", gameId);

        try
        {
            var moves = await _moveService.GetMovesByGameIdAsync(gameId);

            if (moves == null || moves.Count == 0)
            {
                _logger.LogWarning("No moves found for game {GameId}.", gameId);
                return NotFound(new { message = "No moves found for this game." });
            }

            var moveDtos = moves.Select(move => new MoveDto
            {
                MoveId = move.MoveId,
                GameId = move.GameId,
                PlayerId = move.PlayerId,
                Row = move.Row,
                Column = move.Column,
                MoveNumber = move.MoveNumber
            }).ToList();

            _logger.LogInformation("Successfully fetched {MoveCount} moves for game {GameId}.", moveDtos.Count, gameId);
            return Ok(new { message = "Moves retrieved successfully", data = moveDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while fetching moves for game {GameId}: {ErrorMessage}", gameId, ex.Message);
            return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
        }
    }
}
