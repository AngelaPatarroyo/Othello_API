using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Othello_API.Dtos;
using Othello_API.Interfaces;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

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

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="registerDto">User registration details.</param>
    /// <returns>Returns success message with user details.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Register a new user", Description = "Creates a new user account.")]
    [SwaggerResponse(200, "User registered successfully", typeof(RegisterDto))]
    [SwaggerResponse(400, "Invalid registration request")]
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

    /// <summary>
    /// Authenticates and logs in a user.
    /// </summary>
    /// <param name="loginDto">User login credentials.</param>
    /// <returns>Returns a JWT token if successful.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Login a user", Description = "Authenticates a user and returns a JWT token.")]
    [SwaggerResponse(200, "Login successful", typeof(string))]
    [SwaggerResponse(401, "Invalid login credentials")]
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

    /// <summary>
    /// Assigns a role to a user (Admin only).
    /// </summary>
    /// <param name="request">The role assignment details.</param>
    /// <returns>Returns success message.</returns>
    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Assign role to user (Admin only)", Description = "Assigns a specified role to a user.")]
    [SwaggerResponse(200, "Role assigned successfully")]
    [SwaggerResponse(404, "User not found")]
    [SwaggerResponse(400, "Failed to assign role")]
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

    /// <summary>
    /// Retrieves all users (Admin only).
    /// </summary>
    /// <returns>Returns a list of users.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Get all users (Admin only)", Description = "Fetches all registered users.")]
    [SwaggerResponse(200, "Successfully retrieved all users", typeof(List<RegisterDto>))]
    [SwaggerResponse(404, "No users available")]
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

    /// <summary>
    /// Updates a user profile.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="dto">Updated user information.</param>
    /// <returns>Returns success message.</returns>
    [HttpPut("{id}")]
    [Authorize] 
    [SwaggerOperation(Summary = "Update user profile", Description = "Updates user profile details. Users can only update their own profile.")]
    [SwaggerResponse(200, "User updated successfully")]
    [SwaggerResponse(403, "Forbidden - User can only update their own profile")]
    [SwaggerResponse(400, "Invalid update request")]
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

    /// <summary>
    /// Deletes a user (Admin only).
    /// </summary>
    /// <param name="id">User ID to delete.</param>
    /// <returns>Returns success message.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete a user (Admin only)", Description = "Deletes a user from the system.")]
    [SwaggerResponse(200, "User deleted successfully")]
    [SwaggerResponse(404, "User not found")]
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
