using Othello_API.Interfaces;
using Othello_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Othello_API.Services
{
    public class LeaderBoardService : ILeaderBoardService
    {
        private readonly ApplicationDbContext _context;

        public LeaderBoardService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get leaderboard with rankings
        public async Task<List<LeaderBoard>> GetLeaderboardAsync()
        {
            return await _context.LeaderBoards
                .OrderByDescending(lb => lb.Ranking) // Sorting by ranking
                .ToListAsync();
        }

        // Get the ranking of a user (returns nullable int to match the nullable Ranking)
        public async Task<int?> GetUserRankingAsync(string userId)
        {
            var userLeaderBoard = await _context.LeaderBoards
                .FirstOrDefaultAsync(lb => lb.UserId == userId);

            // Return the ranking or null if the user is not found or ranking is null
            return userLeaderBoard?.Ranking; // null will be returned if no ranking is found
        }
    }
}
