using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Othello_API.Interfaces;
using Othello_API.Dtos;
using Othello_API.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Requires authentication
public class MoveController : ControllerBase
{
    private readonly IMoveService _moveService;

    public MoveController(IMoveService moveService)
    {
        _moveService = moveService;
    }

    // Make a move
    [Authorize]
    [HttpPost("{gameId}/move")]
    public async Task<IActionResult> MakeMove(int gameId, [FromBody] MoveDto moveDto)
    {
        try
        {
            var move = await _moveService.MakeMoveAsync(gameId, moveDto);
            if (move == null) return NotFound("Game not found.");

            // Convert move to MoveDto (since we are not using MoveResponseDto anymore)
            var response = new MoveDto
            {
                MoveId = move.MoveId,
                GameId = move.GameId,
                PlayerId = move.PlayerId,
                Row = move.Row,
                Column = move.Column,
                MoveNumber = move.MoveNumber
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // Get all moves for a game
    [HttpGet("{gameId}/moves")]
    public async Task<IActionResult> GetMoves(int gameId)
    {
        try
        {
            var moves = await _moveService.GetMovesByGameIdAsync(gameId);

            if (moves == null || moves.Count == 0)
                return NotFound("No moves found for this game.");

            // Convert moves to MoveDto list
            var moveDtos = moves.Select(move => new MoveDto
            {
                MoveId = move.MoveId,
                GameId = move.GameId,
                PlayerId = move.PlayerId,
                Row = move.Row,
                Column = move.Column,
                MoveNumber = move.MoveNumber
            }).ToList();

            return Ok(moveDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
