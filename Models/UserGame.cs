using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Othello_API.Models; 

namespace Othello_API.Models
{
    public class UserGame
    {
        [Key]
        public int UserGameId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int GameId { get; set; }

        public bool IsWinner { get; set; }

        public int TotalWins { get; set; } = 0;
        public int TotalLosses { get; set; } = 0;
        public int TotalGames { get; set; } = 0;

        
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

 
        [ForeignKey("GameId")]
        public Game? Game { get; set; }
    }
}
