using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class LeaderBoardController : ControllerBase
{
    private readonly ILeaderBoardService _leaderBoardService;
    private readonly ILogger<LeaderBoardController> _logger;

    public LeaderBoardController(ILeaderBoardService leaderBoardService, ILogger<LeaderBoardController> logger)
    {
        _leaderBoardService = leaderBoardService;
        _logger = logger;
    }

    // Get leaderboard rankings
    [HttpGet]
    public async Task<IActionResult> GetLeaderBoard()
    {
        try
        {
            _logger.LogInformation("Fetching leaderboard rankings.");

            var leaderboard = await _leaderBoardService.GetLeaderboardAsync();

            if (leaderboard == null || leaderboard.Count == 0)
            {
                _logger.LogWarning("No rankings found in the leaderboard.");
                return NotFound(new { message = "No rankings found" });
            }

            _logger.LogInformation("Successfully fetched {LeaderBoardCount} rankings.", leaderboard.Count);
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching leaderboard rankings.");
            return StatusCode(500, new { message = "Internal Server Error" });
        }
    }

    // Get ranking for a specific user
    [HttpGet("{userId}")]
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
            return Ok(ranking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching ranking for user ID {UserId}.", userId);
            return StatusCode(500, new { message = "Internal Server Error" });
        }
    }
}
