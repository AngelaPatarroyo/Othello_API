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
            _logger.LogInformation("Attempting move in GameId {GameId} by User {UserId} at Row {Row}, Column {Column}.", 
                gameId, moveDto.PlayerId, moveDto.Row, moveDto.Column);

            var game = await _context.Games
                .Include(g => g.Moves)
                .FirstOrDefaultAsync(g => g.GameId == gameId);

            if (game == null)
            {
                _logger.LogError("Game with ID {GameId} not found.", gameId);
                throw new KeyNotFoundException("Game not found.");
            }

            await _context.Entry(game).Collection(g => g.Moves).LoadAsync();

            if (game.Moves.Any(m => m.Row == moveDto.Row && m.Column == moveDto.Column))
            {
                _logger.LogWarning("Move at Row {Row}, Column {Column} is already occupied in GameId {GameId}.", 
                    moveDto.Row, moveDto.Column, gameId);
                throw new InvalidOperationException("Move position is already occupied.");
            }

            int moveNumber = game.Moves.Count + 1;

            var move = new Move
            {
                GameId = gameId,
                PlayerId = moveDto.PlayerId,
                Row = moveDto.Row,
                Column = moveDto.Column,
                MoveNumber = moveNumber
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Moves.Add(move);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Move {MoveNumber} successfully made for GameId {GameId} by User {UserId}.", 
                    moveNumber, gameId, moveDto.PlayerId);

                return move;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error making move for GameId {GameId} by User {UserId}.", gameId, moveDto.PlayerId);
                throw;
            }
        }

        public async Task<List<Move>> GetMovesByGameIdAsync(int gameId)
        {
            _logger.LogInformation("Fetching all moves for GameId {GameId}.", gameId);

            var moves = await _context.Moves
                .Where(m => m.GameId == gameId)
                .OrderBy(m => m.MoveNumber)
                .ToListAsync();

            if (!moves.Any())
            {
                _logger.LogWarning("No moves found for GameId {GameId}.", gameId);
            }
            else
            {
                _logger.LogInformation("Retrieved {MoveCount} moves for GameId {GameId}.", moves.Count, gameId);
            }

            return moves;
        }
    }
}
