using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Othello_API.Dtos;
using Othello_API.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Othello_API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
        }

        public async Task<ApplicationUser?> RegisterUserAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Attempting to register user with username: {Username}, email: {Email}.", registerDto.UserName, registerDto.Email);

            var user = new ApplicationUser { UserName = registerDto.UserName, Email = registerDto.Email };
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} registered successfully.", registerDto.UserName);
                return user;
            }
            else
            {
                _logger.LogError("Failed to register user {Username}. Errors: {Errors}", registerDto.UserName, string.Join(", ", result.Errors));
                return null;
            }
        }

        public async Task<string?> LoginUserAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Attempting login for user with username/email: {UsernameOrEmail}.", loginDto.UserName ?? loginDto.Email);

            ApplicationUser? user = null;

            if (!string.IsNullOrEmpty(loginDto.UserName))
            {
                user = await _userManager.FindByNameAsync(loginDto.UserName);
            }

            if (user == null && !string.IsNullOrEmpty(loginDto.Email))
            {
                user = await _userManager.FindByEmailAsync(loginDto.Email);
            }

            if (user == null)
            {
                _logger.LogWarning("User {UsernameOrEmail} not found.", loginDto.UserName ?? loginDto.Email);
                return null;
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UsernameOrEmail} logged in successfully.", loginDto.UserName ?? loginDto.Email);
                return GenerateJwtToken(user);
            }
            else
            {
                _logger.LogWarning("Failed login attempt for user {UsernameOrEmail}.", loginDto.UserName ?? loginDto.Email);
                return null;
            }
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            _logger.LogInformation("Generating JWT token for user {UsernameOrEmail}.", user.UserName ?? user.Email);

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
            _logger.LogInformation("JWT token generated for user {UsernameOrEmail}.", user.UserName ?? user.Email);
            return tokenHandler.WriteToken(token);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            _logger.LogInformation("Fetching all users.");
            return await _userManager.Users.ToListAsync();
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUserDto dto)
        {
            _logger.LogInformation("Updating user with ID {UserId}.", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for update.", id);
                return false;
            }

            if (!string.IsNullOrEmpty(dto.UserName))
            {
                user.UserName = dto.UserName;
            }

            if (!string.IsNullOrEmpty(dto.Email))
            {
                user.Email = dto.Email;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} updated successfully.", id);
                return true;
            }
            else
            {
                _logger.LogError("Failed to update user with ID {UserId}. Errors: {Errors}", id, string.Join(", ", result.Errors));
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            _logger.LogInformation("Attempting to delete user with ID {UserId}.", id);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion.", id);
                return false;
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
                return true;
            }
            else
            {
                _logger.LogError("Failed to delete user with ID {UserId}. Errors: {Errors}", id, string.Join(", ", result.Errors));
                return false;
            }
        }
    }
}

