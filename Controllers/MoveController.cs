using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Othello_API.Interfaces;
using Othello_API.Dtos;


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

    // Make a move
    [Authorize]
    [HttpPost("{gameId}/move")]
    public async Task<IActionResult> MakeMove(int gameId, [FromBody] MoveDto moveDto)
    {
        _logger.LogInformation("Attempting to make a move for game {GameId} by player {PlayerId}.", gameId, moveDto.PlayerId);

        try
        {
            var move = await _moveService.MakeMoveAsync(gameId, moveDto);
            if (move == null)
            {
                _logger.LogWarning("Game {GameId} not found when trying to make a move.", gameId);
                return NotFound("Game not found.");
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
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while making a move for game {GameId}: {ErrorMessage}", gameId, ex.Message);
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Get all moves for a game
    [HttpGet("{gameId}/moves")]
    public async Task<IActionResult> GetMoves(int gameId)
    {
        _logger.LogInformation("Fetching all moves for game {GameId}.", gameId);

        try
        {
            var moves = await _moveService.GetMovesByGameIdAsync(gameId);

            if (moves == null || moves.Count == 0)
            {
                _logger.LogWarning("No moves found for game {GameId}.", gameId);
                return NotFound("No moves found for this game.");
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
            return Ok(moveDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while fetching moves for game {GameId}: {ErrorMessage}", gameId, ex.Message);
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
