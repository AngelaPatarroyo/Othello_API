
using Othello_API.Interfaces;
using Othello_API.Models;
using Othello_API.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace Othello_API.Services
{
    public class MoveService : IMoveService
    {
        private readonly ApplicationDbContext _context;

        public MoveService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a move to the game
        public async Task<Move> MakeMoveAsync(int gameId, MoveDto moveDto)
        {
            var game = await _context.Games
                .Include(g => g.Moves)  // Include moves in the game
                .FirstOrDefaultAsync(g => g.GameId == gameId);

            if (game == null)
            {
                throw new Exception("Game not found.");
            }

            // Create the move object based on the MoveDto
            var move = new Move
            {
                GameId = gameId,
                PlayerId = moveDto.PlayerId,
                Row = moveDto.Row,
                Column = moveDto.Column
            };

            // Add the move to the game's list of moves
            game.Moves.Add(move);
            await _context.SaveChangesAsync();

            return move;
        }

        // Get all moves for a specific game
        public async Task<List<Move>> GetMovesByGameIdAsync(int gameId)
        {
            return await _context.Moves
                .Where(m => m.GameId == gameId)
                .ToListAsync();
        }
    }
}
