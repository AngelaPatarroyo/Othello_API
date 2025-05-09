using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Othello_API.Models;

public class ApplicationUser : IdentityUser
{
    [JsonIgnore]
    public virtual List<UserGame> UserGames { get; set; } = new();

    [JsonIgnore]
    public virtual LeaderBoard? LeaderBoard { get; set; }

    //Add these properties for tracking game results
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public int Draws { get; set; } = 0;
    public int TotalGames { get; set; } = 0;
    public double WinRate { get; set; } = 0;
}
