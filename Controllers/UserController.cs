using Microsoft.AspNetCore.Mvc;
using Othello_API.Models;
using Othello_API.Dtos;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var user = await _userService.RegisterUserAsync(registerDto);

        if (user == null)
        {
            return BadRequest(new { message = "Registration failed" });
        }

        return Ok(new { message = "User registered successfully", user.Id, user.UserName, user.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _userService.LoginUserAsync(loginDto);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid login credentials" });
        }

        return Ok(new { message = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users.Select(user => new { user.Id, user.UserName, user.Email }));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        var success = await _userService.UpdateUserAsync(id, dto);
        if (!success) return BadRequest("Update failed");

        return Ok("User updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success) return NotFound("User not found");

        return Ok("User deleted successfully");
    }

}
