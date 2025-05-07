using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Othello_API.Dtos;
using Othello_API.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserController> _logger;
    private readonly IConfiguration _config;

    public UserController(
        IUserService userService,
        UserManager<ApplicationUser> userManager,
        ILogger<UserController> logger,
        IConfiguration config)
    {
        _userService = userService;
        _userManager = userManager;
        _logger = logger;
        _config = config;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (registerDto == null || !ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration attempt.");
            return BadRequest(new { message = "Invalid registration request" });
        }

        var user = await _userService.RegisterUserAsync(registerDto);

        if (user == null)
        {
            _logger.LogWarning("Registration failed.");
            return BadRequest(new { message = "Registration failed" });
        }

        _logger.LogInformation("User {Username} registered successfully.", user.UserName);
        return Ok(new { message = "User registered successfully", user.Id, user.UserName, user.Email });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (loginDto == null || !ModelState.IsValid || !loginDto.IsValid())
        {
            _logger.LogWarning("Invalid login attempt.");
            return BadRequest(new { message = "Invalid login credentials" });
        }

        ApplicationUser? user = !string.IsNullOrEmpty(loginDto.Email)
            ? await _userManager.FindByEmailAsync(loginDto.Email)
            : await _userManager.FindByNameAsync(loginDto.UserName!);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            _logger.LogWarning("Login failed for user {User}.", loginDto.Email ?? loginDto.UserName);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!);
        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            expires: DateTime.UtcNow.AddHours(2),
            claims: authClaims,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo,
            user = new
            {
                user.Id,
                user.UserName,
                user.Email,
                Role = roles.FirstOrDefault()
            }
        });
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] RoleAssignmentRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return NotFound($"User with email {request.Email} not found.");
        }

        var result = await _userManager.AddToRoleAsync(user, request.Role);
        if (!result.Succeeded)
        {
            return BadRequest("Failed to assign role.");
        }

        return Ok(new { message = $"Role {request.Role} assigned to {request.Email}" });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Admin fetching all users.");

        var users = _userManager.Users.ToList();
        if (users.Count == 0)
        {
            _logger.LogWarning("No users found.");
            return NotFound("No users available.");
        }

        var result = new List<object>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new
            {
                user.Id,
                user.UserName,
                user.Email,
                Role = roles.FirstOrDefault() ?? "None"
            });
        }

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var loggedInUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        _logger.LogInformation("Attempting to update user with ID {UserId}.", id);

        if (dto == null || !ModelState.IsValid)
        {
            _logger.LogWarning("Invalid update request.");
            return BadRequest("Update request is empty or invalid.");
        }

        if (loggedInUserId != id && loggedInUserRole != "Admin")
        {
            _logger.LogWarning("User {UserId} attempted to update another user's profile.", loggedInUserId);
            return Forbid("You can only update your own profile.");
        }

        dto.RestrictUpdates();

        if (!dto.IsValid())
        {
            return BadRequest("At least one field (UserName, Email, or NewPassword) must be provided.");
        }

        var success = await _userService.UpdateUserAsync(id, dto);
        if (!success)
        {
            _logger.LogWarning("Update failed for user with ID {UserId}.", id);
            return BadRequest("Update failed");
        }

        _logger.LogInformation("User updated successfully.");
        return Ok("User updated successfully");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        _logger.LogInformation("Admin attempting to delete user with ID {UserId}.", id);

        var success = await _userService.DeleteUserAsync(id);
        if (!success)
        {
            _logger.LogWarning("User not found for deletion.");
            return NotFound("User not found");
        }

        _logger.LogInformation("User deleted successfully.");
        return Ok("User deleted successfully");
    }
}
