using Othello_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Othello_API.Models;


namespace Othello_API.Services
{
    public class UserGameService : IUserGameService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserGameService> _logger;

        public UserGameService(ApplicationDbContext context, ILogger<UserGameService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserGame>> GetAllUserGamesAsync()
        {
            _logger.LogInformation("Fetching all user games.");
            var userGames = await _context.UserGames
                .AsNoTracking()
                .Include(ug => ug.User)
                .Include(ug => ug.Game)
                .ToListAsync();

            _logger.LogInformation("Fetched {UserGameCount} user games.", userGames.Count);
            return userGames;
        }

        public async Task<UserGame> GetUserGameByIdAsync(int id)
        {
            _logger.LogInformation("Fetching user game with ID {UserGameId}.", id);

            if (_context == null || _context.UserGames == null)
            {
                _logger.LogError("Database context or UserGames table is not available.");
                throw new Exception("Database context or UserGames table is not available.");
            }

            var userGame = await _context.UserGames
                .Include(ug => ug.User)
                .Include(ug => ug.Game)
                .FirstOrDefaultAsync(ug => ug.UserGameId == id);

            if (userGame == null)
            {
                _logger.LogWarning("UserGame with ID {UserGameId} not found.", id);
            }

            return userGame ?? throw new KeyNotFoundException($"UserGame with ID {id} not found.");
        }

        public async Task<UserGame> CreateUserGameAsync(UserGame userGame)
        {
            _logger.LogInformation("Attempting to create UserGame for UserId: {UserId}, GameId: {GameId}.", userGame.UserId, userGame.GameId);

            // Validate User Exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userGame.UserId);
            if (!userExists)
            {
                _logger.LogError("User with ID {UserId} not found.", userGame.UserId);
                throw new KeyNotFoundException("User not found.");
            }

            // Validate Game Exists
            var gameExists = await _context.Games.AnyAsync(g => g.GameId == userGame.GameId);
            if (!gameExists)
            {
                _logger.LogError("Game with ID {GameId} not found.", userGame.GameId);
                throw new KeyNotFoundException("Game not found.");
            }

            _context.UserGames.Add(userGame);
            await _context.SaveChangesAsync();

            _logger.LogInformation("UserGame created successfully with ID {UserGameId}.", userGame.UserGameId);
            return userGame;
        }

        public async Task<UserGame> UpdateUserGameAsync(int id, UserGame userGame)
        {
            _logger.LogInformation("Attempting to update UserGame with ID {UserGameId}.", id);

            var existingUserGame = await _context.UserGames.FindAsync(id);
            if (existingUserGame == null)
            {
                _logger.LogWarning("UserGame with ID {UserGameId} not found for update.", id);
                throw new KeyNotFoundException($"UserGame with ID {id} not found.");
            }

            existingUserGame.TotalWins = userGame.TotalWins;
            existingUserGame.TotalLosses = userGame.TotalLosses;
            existingUserGame.TotalGames = userGame.TotalGames;

            _context.UserGames.Update(existingUserGame);
            await _context.SaveChangesAsync();

            _logger.LogInformation("UserGame with ID {UserGameId} updated successfully.", id);
            return existingUserGame;
        }

        public async Task<bool> DeleteUserGameAsync(int id)
        {
            _logger.LogInformation("Attempting to delete UserGame with ID {UserGameId}.", id);

            var userGame = await _context.UserGames.FindAsync(id);
            if (userGame == null)
            {
                _logger.LogWarning("UserGame with ID {UserGameId} not found for deletion.", id);
                return false;
            }

            _context.UserGames.Remove(userGame);
            await _context.SaveChangesAsync();

            _logger.LogInformation("UserGame with ID {UserGameId} deleted successfully.", id);
            return true;
        }
    }
}
