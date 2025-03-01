using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Othello_API.Models;

public class UserGame
{
    [Key]
    public int UserGameId { get; set; }  // Primary Key

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; } = null!;  // Foreign Key (Non-Nullable)

    public virtual ApplicationUser? User { get; set; }  // Navigation Property (Nullable)

    [Required]
    [ForeignKey("Game")]
    public int GameId { get; set; }  // Foreign Key (Non-Nullable)

    public virtual Game? Game { get; set; }  // Navigation Property (Nullable)

    public int TotalWins { get; set; } = 0;
    public int TotalLosses { get; set; } = 0;
    public int TotalGames { get; set; } = 0;
}
