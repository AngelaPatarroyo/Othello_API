using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Othello_API.Dtos;
using Othello_API.Models;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<ApplicationUser?> RegisterUserAsync(RegisterDto registerDto)
    {
        var user = new ApplicationUser { UserName = registerDto.Username, Email = registerDto.Email };
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        return result.Succeeded ? user : null;
    }

    public async Task<string?> LoginUserAsync(LoginDto loginDto)
    {
        ApplicationUser? user = null;

        if (!string.IsNullOrEmpty(loginDto.Username))
        {
            user = await _userManager.FindByNameAsync(loginDto.Username);
        }

        if (user == null && !string.IsNullOrEmpty(loginDto.Email))
        {
            user = await _userManager.FindByEmailAsync(loginDto.Email);
        }

        if (user == null)
        {
            return null;
        }

        var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

        return result.Succeeded ? "Login successful" : null;
    }

    public async Task<List<ApplicationUser>> GetAllUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }
    public async Task<bool> UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        if (!string.IsNullOrEmpty(dto.UserName))
        {
            user.UserName = dto.UserName;
        }

        if (!string.IsNullOrEmpty(dto.Email))
        {
            user.Email = dto.Email;
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }



}
