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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApplicationUser?> RegisterUserAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Registering user with username: {Username}, email: {Email}.", registerDto.UserName, registerDto.Email);

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

        public async Task<string?> LoginUserAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Attempting login for user: {UsernameOrEmail}.", loginDto.UserName ?? loginDto.Email);

            ApplicationUser? user = null;
            if (!string.IsNullOrEmpty(loginDto.UserName))
                user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null && !string.IsNullOrEmpty(loginDto.Email))
                user = await _userManager.FindByEmailAsync(loginDto.Email);

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

        private string GenerateJwtToken(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");

            _logger.LogInformation("Generating JWT token for user {UsernameOrEmail}.", user.UserName ?? user.Email);

            var secretKey = _config["JwtSettings:Secret"];
            var issuer = _config["JwtSettings:Issuer"];
            var audience = _config["JwtSettings:Audience"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new Exception("JWT configuration is missing! Check appsettings.json");

            byte[] key;
            if (secretKey.Length >= 44 && secretKey.EndsWith("="))
            {
                try
                {
                    key = Convert.FromBase64String(secretKey);
                    _logger.LogInformation("Decoded Base64 JWT Secret. Key length: {Length} bytes.", key.Length);
                }
                catch (FormatException)
                {
                    _logger.LogWarning("JWT Secret was expected to be Base64 but failed decoding. Using as plain text.");
                    key = Encoding.UTF8.GetBytes(secretKey);
                }
            }
            else
            {
                _logger.LogInformation("Using plain text JWT Secret.");
                key = Encoding.UTF8.GetBytes(secretKey);
            }

            if (key.Length < 32)
            {
                throw new InvalidOperationException($"JWT Secret key must be at least 32 bytes, but found {key.Length} bytes.");
            }

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

        public Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateUserAsync(string id, UpdateUserDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}