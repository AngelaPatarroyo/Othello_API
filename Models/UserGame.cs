using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Othello_API.Models
{
    public class UserGame
    {
        [Key]
        public int UserGameId { get; set; }

        [Required]
        [ForeignKey("User")]
        public string? UserId { get; set; } // IdentityUser uses string ID
        public virtual ApplicationUser? User { get; set; } 

        [Required]
        [ForeignKey("Game")]
        public int GameId { get; set; }
        public virtual Game? Game { get; set; }

        public int TotalWins { get; set; } = 0;
        public int TotalLosses { get; set; } = 0;
        public int TotalGames { get; set; } = 0;
    }
}
