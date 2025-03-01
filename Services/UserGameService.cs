using Othello_API.Interfaces;
using Othello_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Othello_API.Services
{
    public class UserGameService : IUserGameService
    {
        private readonly ApplicationDbContext _context;

        public UserGameService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<UserGame>> GetAllUserGamesAsync()
        {
            return await _context.UserGames
                .AsNoTracking()
                .Include(ug => ug.User)
                .Include(ug => ug.Game)
                .ToListAsync();
        }

        public async Task<UserGame> GetUserGameByIdAsync(int id)
        {
            if (_context == null || _context.UserGames == null)
            {
                throw new Exception("Database context or UserGames table is not available.");
            }
#pragma warning disable CS8603 // Possible null reference return.
            return await _context.UserGames
                .Include(ug => ug.User)
                .Include(ug => ug.Game)
                .FirstOrDefaultAsync(ug => ug.UserGameId == id);
#pragma warning restore CS8603 // Possible null reference return.
        }



        public async Task<UserGame> CreateUserGameAsync(UserGame userGame)
        {
            // Validate User Exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userGame.UserId);
            if (!userExists)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Validate Game Exists
            var gameExists = await _context.Games.AnyAsync(g => g.GameId == userGame.GameId);
            if (!gameExists)
            {
                throw new KeyNotFoundException("Game not found.");
            }

            _context.UserGames.Add(userGame);
            await _context.SaveChangesAsync();
            return userGame;
        }

        public async Task<UserGame> UpdateUserGameAsync(int id, UserGame userGame)
        {
            var existingUserGame = await _context.UserGames.FindAsync(id);

            if (existingUserGame == null)
            {
                throw new KeyNotFoundException($"UserGame with ID {id} not found.");
            }

            existingUserGame.TotalWins = userGame.TotalWins;
            existingUserGame.TotalLosses = userGame.TotalLosses;
            existingUserGame.TotalGames = userGame.TotalGames;

            _context.UserGames.Update(existingUserGame);
            await _context.SaveChangesAsync();
            return existingUserGame;
        }

        public async Task<bool> DeleteUserGameAsync(int id)
        {
            var userGame = await _context.UserGames.FindAsync(id);
            if (userGame == null)
            {
                return false;
            }

            _context.UserGames.Remove(userGame);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}