using Othello_API.Dtos;


namespace Othello_API.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser?> RegisterUserAsync(RegisterDto registerDto);
        Task<string?> LoginUserAsync(LoginDto loginDto);
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(string id, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(string id);
    }
}
