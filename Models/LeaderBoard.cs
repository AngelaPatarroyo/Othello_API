using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Othello_API.Models
{
    public class LeaderBoard
    {
        [Key]
        public int LeaderBoardId { get; set; } // Unique Leaderboard ID

        [Required]
        [ForeignKey("User")]
        public string? UserId { get; set; } // IdentityUser uses string ID

        public int? Ranking { get; set; }

        // Relationship with ApplicationUser
        public virtual ApplicationUser? User { get; set; }
    }
}
