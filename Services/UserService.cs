using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Othello_API.Dtos;
using Othello_API.Interfaces;
using Othello_API.Models;
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
        private readonly ApplicationDbContext _dbContext;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            ILogger<UserService> logger,
            ApplicationDbContext dbContext)  // Inject _dbContext
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        //  Register User
        public async Task<ApplicationUser?> RegisterUserAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Registering user: {Username}, Email: {Email}", registerDto.UserName, registerDto.Email);

            var user = new ApplicationUser { UserName = registerDto.UserName, Email = registerDto.Email };
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} registered successfully.", registerDto.UserName);
                return user;
            }

            _logger.LogError("Failed to register user {Username}. Errors: {Errors}", registerDto.UserName, string.Join(", ", result.Errors));
            return null;
        }

        // Login User
        public async Task<string?> LoginUserAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Attempting login for user: {UsernameOrEmail}.", loginDto.UserName ?? loginDto.Email);

            var user = !string.IsNullOrEmpty(loginDto.UserName)
                ? await _userManager.FindByNameAsync(loginDto.UserName)
                : await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                _logger.LogWarning("User {UsernameOrEmail} not found.", loginDto.UserName ?? loginDto.Email);
                return null;
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed login attempt for user {UsernameOrEmail}.", loginDto.UserName ?? loginDto.Email);
                return null;
            }

            _logger.LogInformation("User {UsernameOrEmail} logged in successfully.", loginDto.UserName ?? loginDto.Email);
            return GenerateJwtToken(user);
        }

        // Generate JWT Token
        private string GenerateJwtToken(ApplicationUser user)
        {
            _logger.LogInformation("Generating JWT token for user {UsernameOrEmail}.", user.UserName ?? user.Email);

            var secretKey = _config["JwtSettings:Secret"];
            var issuer = _config["JwtSettings:Issuer"];
            var audience = _config["JwtSettings:Audience"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new Exception("JWT configuration is missing! Check appsettings.json");

            byte[] key = Encoding.UTF8.GetBytes(secretKey);

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
            _logger.LogInformation("JWT token successfully generated for user {UsernameOrEmail}.", user.UserName ?? user.Email);
            return tokenHandler.WriteToken(token);
        }

        // Get All Users (Admin Only)
        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        //  Update User
        public async Task<bool> UpdateUserAsync(string id, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                return false;
            }

            if (!string.IsNullOrEmpty(dto.UserName))
                user.UserName = dto.UserName;

            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to update user {UserId}. Errors: {Errors}", id, string.Join(", ", result.Errors));
                return false;
            }

            _logger.LogInformation("User {UserId} updated successfully.", id);
            return true;
        }

        //  Delete User (Fix FOREIGN KEY issue)
        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for deletion.", id);
                return false;
            }

            //  Remove related UserGames
            var userGames = await _dbContext.UserGames.Where(ug => ug.UserId == id).ToListAsync();
            if (userGames.Any())
            {
                _dbContext.UserGames.RemoveRange(userGames);
                await _dbContext.SaveChangesAsync();
            }

            //  Remove LeaderBoard entry if exists
            var leaderBoardEntry = await _dbContext.LeaderBoard.FirstOrDefaultAsync(lb => lb.PlayerId == id);
            if (leaderBoardEntry != null)
            {
                _dbContext.LeaderBoard.Remove(leaderBoardEntry);
                await _dbContext.SaveChangesAsync();
            }

            //  Update Game table to NULL instead of deleting (to prevent FK issues)
            var gamesWhereUserIsPlayer1 = await _dbContext.Games.Where(g => g.Player1Id == id).ToListAsync();
            foreach (var game in gamesWhereUserIsPlayer1)
            {
                game.Player1Id = null;
            }

            var gamesWhereUserIsPlayer2 = await _dbContext.Games.Where(g => g.Player2Id == id).ToListAsync();
            foreach (var game in gamesWhereUserIsPlayer2)
            {
                game.Player2Id = null;
            }

            await _dbContext.SaveChangesAsync();

            //  Remove user from roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            //  Now delete the user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to delete user {UserId}. Errors: {Errors}", id, string.Join(", ", result.Errors));
                return false;
            }

            _logger.LogInformation("User {UserId} deleted successfully.", id);
            return true;
        }
    }
}
