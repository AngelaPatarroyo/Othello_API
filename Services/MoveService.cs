using Othello_API.Interfaces;
using Othello_API.Models;
using Othello_API.Dtos;
using Microsoft.EntityFrameworkCore;


namespace Othello_API.Services
{
    public class MoveService : IMoveService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MoveService> _logger;

        public MoveService(ApplicationDbContext context, ILogger<MoveService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Move> MakeMoveAsync(int gameId, MoveDto moveDto)
        {
            _logger.LogInformation("Attempting to make a move in GameId {GameId} by Player {PlayerId}.", gameId, moveDto.PlayerId);

            var game = await _context.Games
                .Include(g => g.Moves)
                .FirstOrDefaultAsync(g => g.GameId == gameId);

            if (game == null)
            {
                _logger.LogError("Game with ID {GameId} not found.", gameId);
                throw new KeyNotFoundException("Game not found.");
            }

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

            _logger.LogInformation("Move {MoveNumber} made successfully for GameId {GameId} by Player {PlayerId}.", moveNumber, gameId, moveDto.PlayerId);

            return move;
        }

        // Get all moves for a specific game
        public async Task<List<Move>> GetMovesByGameIdAsync(int gameId)
        {
            _logger.LogInformation("Fetching all moves for GameId {GameId}.", gameId);

            var moves = await _context.Moves
                .Where(m => m.GameId == gameId)
                .OrderBy(m => m.MoveNumber)
                .ToListAsync();

            if (moves.Count == 0)
            {
                _logger.LogWarning("No moves found for GameId {GameId}.", gameId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved {MoveCount} moves for GameId {GameId}.", moves.Count, gameId);
            }

            return moves;
        }
    }
}
