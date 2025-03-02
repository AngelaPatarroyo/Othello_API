using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Othello_API.Dtos;
using Othello_API.Interfaces;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, UserManager<ApplicationUser> userManager, ILogger<UserController> logger)
    {
        _userService = userService;
        _userManager = userManager;
        _logger = logger;
    }

    // OPEN TO ALL - User Registration
    [HttpPost("register")]
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

    // OPEN TO ALL - User Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (loginDto == null || !ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login attempt.");
            return BadRequest(new { message = "Invalid login credentials" });
        }

        var token = await _userService.LoginUserAsync(loginDto);

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Invalid login attempt.");
            return Unauthorized(new { message = "Invalid login credentials" });
        }

        _logger.LogInformation("User logged in successfully.");
        return Ok(new { token });
    }

    // ADMIN ONLY - Assign Role to a User
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

    // ADMIN ONLY - Get All Users
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Admin fetching all users.");

        var users = await _userService.GetAllUsersAsync();

        if (users == null || users.Count == 0)
        {
            _logger.LogWarning("No users found.");
            return NotFound("No users available.");
        }

        return Ok(users.Select(user => new { user.Id, user.UserName, user.Email }));
    }

    // AUTHORIZED USERS & ADMINS - Update User Profile
    [HttpPut("{id}")]
    [Authorize] // Only logged-in users can update
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var loggedInUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        _logger.LogInformation("Attempting to update user with ID {UserId}.", id);

        if (dto == null || !dto.IsValid())
        {
            _logger.LogWarning("Invalid update request.");
            return BadRequest("Update request is empty or invalid.");
        }

        // If Admin, they can update any user
        // If Normal User, they can only update their own profile
        if (loggedInUserId != id && loggedInUserRole != "Admin")
        {
            return Forbid("You can only update your own profile.");
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

    // ADMIN ONLY - Delete Any User
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

