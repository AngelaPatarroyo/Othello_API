using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Othello_API.Dtos;
using Othello_API.Interfaces;
using Othello_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Othello_API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config ?? throw new ArgumentNullException(nameof(config));
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
                return null; // User not found
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

            // Correct: Return the generated JWT token instead
            return result.Succeeded ? GenerateJwtToken(user) : null;
        }


        private string GenerateJwtToken(ApplicationUser user)
{
    if (user == null)
    {
        throw new ArgumentNullException(nameof(user), "User object cannot be null.");
    }

    var secretKey = _config["JwtSettings:Secret"];
    var issuer = _config["JwtSettings:Issuer"];
    var audience = _config["JwtSettings:Audience"];

    if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
    {
        throw new Exception("JWT configuration is missing! Check appsettings.json");
    }

    var key = Encoding.UTF8.GetBytes(secretKey);
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "UnknownUser"),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, "Player")
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        Issuer = issuer, 
        Audience = audience, 
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
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
}
