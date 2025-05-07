
using Othello_API.Models;

namespace Othello_API.Interfaces
{
    public interface IUserGameRepository
    {
        Task<IEnumerable<UserGame>> GetAllUserGamesAsync();
        Task<UserGame> GetUserGameByIdAsync(int id);
        Task<UserGame> CreateUserGameAsync(UserGame userGame);
        Task<UserGame> UpdateUserGameAsync(int id, UserGame userGame);
        Task<bool> DeleteUserGameAsync(int id);
    }
}
