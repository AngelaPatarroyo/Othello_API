namespace Othello_API.Interfaces
{
    using Othello_API.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ILeaderBoardService
    {
        Task<List<LeaderBoard>> GetLeaderboardAsync();
        
        
        Task<int?> GetUserRankingAsync(string userId);
    }
}
