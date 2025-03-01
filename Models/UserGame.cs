using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Othello_API.Models;

public class UserGame
{
    [Key]
    public int UserGameId { get; set; } 

    [Required]
    public string UserId { get; set; } = null!; 

    [Required]
    public int GameId { get; set; }  


    public virtual ApplicationUser User { get; set; } = null!;  
    public virtual Game Game { get; set; } = null!;  

    // Extra Stats
    public int TotalWins { get; set; } = 0;
    public int TotalLosses { get; set; } = 0;
    public int TotalGames { get; set; } = 0;
}
