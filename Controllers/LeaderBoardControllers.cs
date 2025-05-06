using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Othello_API.DTOs;

[Route("api/[controller]")]
[ApiController]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderBoardService _leaderBoardService;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(ILeaderBoardService leaderBoardService, ILogger<LeaderboardController> logger)
    {
        _leaderBoardService = leaderBoardService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the full leaderboard rankings. (Admin only)
    /// </summary>
    /// <returns>Returns a list of users ranked by performance.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")] //  Full leaderboard restricted to Admins only
    [SwaggerOperation(Summary = "Get full leaderboard (Admin only)", Description = "Fetches the full leaderboard rankings. Requires Admin role.")]
    [SwaggerResponse(200, "Successfully retrieved leaderboard rankings", typeof(List<LeaderBoardDto>))]
    [SwaggerResponse(404, "No rankings found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> GetLeaderBoard()
    {
        try
        {
            _logger.LogInformation("Admin fetching full leaderboard rankings.");

            var leaderboard = await _leaderBoardService.GetLeaderboardAsync();

            if (leaderboard == null || leaderboard.Count == 0)
            {
                _logger.LogWarning("No rankings found in the leaderboard.");
                return NotFound(new { message = "No rankings found" });
            }

            _logger.LogInformation("Successfully fetched {LeaderBoardCount} rankings.", leaderboard.Count);
            return Ok(new { message = "Leaderboard retrieved successfully", data = leaderboard });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching leaderboard rankings.");
            return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves the ranking for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose ranking is to be retrieved.</param>
    /// <returns>Returns the ranking details for the specified user.</returns>
    [HttpGet("{userId}")]
    [SwaggerOperation(Summary = "Get user ranking", Description = "Fetches the ranking for a specific user by their ID.")]
    [SwaggerResponse(200, "Successfully retrieved user ranking", typeof(LeaderBoardDto))]
    [SwaggerResponse(404, "User ranking not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> GetUserRanking(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching ranking for user with ID {UserId}.", userId);

            var ranking = await _leaderBoardService.GetUserRankingAsync(userId);

            if (ranking == null)
            {
                _logger.LogWarning("Ranking not found for user with ID {UserId}.", userId);
                return NotFound(new { message = $"Ranking not found for user with ID: {userId}" });
            }

            _logger.LogInformation("Successfully fetched ranking for user with ID {UserId}.", userId);
            return Ok(new { message = "User ranking retrieved successfully", data = ranking });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching ranking for user ID {UserId}.", userId);
            return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
        }
    }
}
