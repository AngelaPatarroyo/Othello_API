using Othello_API.Models;
using Othello_API.Dtos;


namespace Othello_API.Interfaces
{
    public interface IMoveService
    {
        Task<Move> MakeMoveAsync(int gameId, MoveDto moveDto);
        Task<List<Move>> GetMovesByGameIdAsync(int gameId);
    }
}
