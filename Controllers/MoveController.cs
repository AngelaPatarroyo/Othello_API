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

    public MoveController(IMoveService moveService)
    {
        _moveService = moveService;
    }

    //  Make a move
    [HttpPost("{gameId}/move")]
    public async Task<IActionResult> MakeMove(int gameId, [FromBody] MoveDto moveDto)
    {
        var move = await _moveService.MakeMoveAsync(gameId, moveDto);
        if (move == null) return BadRequest("Invalid move");
        return Ok(move);
    }

    // 🎯 Get all moves for a game
    [HttpGet("{gameId}/moves")]
    public async Task<IActionResult> GetMoves(int gameId)
    {
        var moves = await _moveService.GetMovesByGameIdAsync(gameId);
        return Ok(moves);
    }
}
