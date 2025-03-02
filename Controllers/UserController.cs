using Microsoft.AspNetCore.Mvc;
using Othello_API.Dtos;
using Othello_API.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // POST: api/User/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (registerDto == null || !ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration attempt: request body is missing or malformed.");
            return BadRequest(new { message = "Invalid registration request" });
        }

        var userNameToLog = string.IsNullOrEmpty(registerDto.UserName) ? "No username provided" : registerDto.UserName;
        _logger.LogInformation("Attempting to register a new user with username {Username}.", userNameToLog);

        var user = await _userService.RegisterUserAsync(registerDto);

        if (user == null)
        {
            _logger.LogWarning("Registration failed for username {Username}.", userNameToLog);
            return BadRequest(new { message = "Registration failed" });
        }

        _logger.LogInformation("User {Username} registered successfully with ID {UserId}.", userNameToLog, user.Id);
        return Ok(new { message = "User registered successfully", user.Id, user.UserName, user.Email });
    }

    // POST: api/User/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (loginDto == null || !ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login attempt: request body is missing or malformed.");
            return BadRequest(new { message = "Invalid login credentials" });
        }

        var usernameOrEmail = !string.IsNullOrEmpty(loginDto.UserName) ? loginDto.UserName : loginDto.Email;
        _logger.LogInformation("User {Username} attempting to log in.", usernameOrEmail);

        var token = await _userService.LoginUserAsync(loginDto);

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Invalid login attempt for username {Username}.", usernameOrEmail);
            return Unauthorized(new { message = "Invalid login credentials" });
        }

        _logger.LogInformation("User {Username} logged in successfully.", usernameOrEmail);
        return Ok(new { token });
    }

    // GET: api/User?page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Fetching all users.");

        var users = await _userService.GetAllUsersAsync();

        if (users == null || users.Count == 0)
        {
            _logger.LogWarning("No users found in the database.");
            return NotFound("No users available.");
        }

        return Ok(users.Select(user => new { user.Id, user.UserName, user.Email }));
    }


    // PUT: api/User/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        _logger.LogInformation("Attempting to update user with ID {UserId}.", id);

        if (dto == null || !dto.IsValid())
        {
            _logger.LogWarning("Invalid update request for user with ID {UserId}.", id);
            return BadRequest("Update request is empty or invalid.");
        }

        var success = await _userService.UpdateUserAsync(id, dto);
        if (!success)
        {
            _logger.LogWarning("Update failed for user with ID {UserId}.", id);
            return BadRequest("Update failed");
        }

        _logger.LogInformation("User with ID {UserId} updated successfully.", id);
        return Ok("User updated successfully");
    }

    // DELETE: api/User/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        _logger.LogInformation("Attempting to delete user with ID {UserId}.", id);

        var success = await _userService.DeleteUserAsync(id);
        if (!success)
        {
            _logger.LogWarning("User with ID {UserId} not found for deletion.", id);
            return NotFound("User not found");
        }

        _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
        return Ok("User deleted successfully");
    }
}
