using Othello_API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Othello_API.Models;



namespace Othello_API.Repositories
{
    public class UserGameRepository : IUserGameRepository
    {
        private readonly ApplicationDbContext _context;

        public UserGameRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<UserGame>> GetAllUserGamesAsync()
        {
            return await _context.UserGames
                .Include(ug => ug.User)
                .Include(ug => ug.Game)
                .ToListAsync();
        }

        public async Task<UserGame> GetUserGameByIdAsync(int id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _context.UserGames
                .Include(ug => ug.User)
                .Include(ug => ug.Game)
                .FirstOrDefaultAsync(ug => ug.UserGameId == id);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task<UserGame> CreateUserGameAsync(UserGame userGame)
        {
            _context.UserGames.Add(userGame);
            await _context.SaveChangesAsync();
            return userGame;
        }

#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
        public async Task<UserGame?> UpdateUserGameAsync(int id, UserGame userGame)
#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
        {
            var existingUserGame = await _context.UserGames.FindAsync(id);
            if (existingUserGame == null)
            {
                return null;
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