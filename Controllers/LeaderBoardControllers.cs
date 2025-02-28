using Microsoft.AspNetCore.Mvc;
using Othello_API.Interfaces;



[Route("api/[controller]")]
[ApiController]
public class LeaderBoardController : ControllerBase
{
    private readonly ILeaderBoardService _leaderBoardService;

    public LeaderBoardController(ILeaderBoardService leaderBoardService)
    {
        _leaderBoardService = leaderBoardService;
    }

    // Get leaderboard rankings
    [HttpGet]
    public async Task<IActionResult> GetLeaderBoard()
    {
        var leaderboard = await _leaderBoardService.GetLeaderboardAsync();

        if (leaderboard == null || leaderboard.Count == 0)
        {
            return NotFound(new { message = "No rankings found" });
        }

        return Ok(leaderboard);
    }

    // Get ranking for a specific user
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserRanking(string userId)
    {
        // Call the service to get the ranking for the user
        var ranking = await _leaderBoardService.GetUserRankingAsync(userId);

        if (ranking == null)
        {
            return NotFound(new { message = $"Ranking not found for user with ID: {userId}" });
        }

        return Ok(ranking);
    }
}
