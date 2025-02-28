using Othello_API.Interfaces;
using Othello_API.Models;
using Othello_API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Othello_API.Services
{
    public class MoveService : IMoveService
    {
        private readonly ApplicationDbContext _context;

        public MoveService(ApplicationDbContext context)
        {
            _context = context;
        }
public async Task<Move> MakeMoveAsync(int gameId, MoveDto moveDto)
{
    var game = await _context.Games
        .Include(g => g.Moves)
        .FirstOrDefaultAsync(g => g.GameId == gameId);

    if (game == null) throw new KeyNotFoundException("Game not found.");

    await _context.Entry(game).Collection(g => g.Moves).LoadAsync();

    int moveNumber = game.Moves.Any() ? game.Moves.Count + 1 : 1;

    var move = new Move
    {
        GameId = gameId,
        PlayerId = moveDto.PlayerId,
        Row = moveDto.Row,
        Column = moveDto.Column,
        MoveNumber = moveNumber
    };

    _context.Moves.Add(move);
    await _context.SaveChangesAsync();

    return move;
}

        // Get all moves for a specific game
        public async Task<List<Move>> GetMovesByGameIdAsync(int gameId)
        {
            return await _context.Moves
                .Where(m => m.GameId == gameId)
                .OrderBy(m => m.MoveNumber)
                .ToListAsync();
        }
    }
}
