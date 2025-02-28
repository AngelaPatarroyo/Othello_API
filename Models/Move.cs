using System.ComponentModel.DataAnnotations;

namespace Othello_API.Models
{
    public class Move
    {
        [Key]
        public int MoveId { get; set; }  // Unique ID for each move

        [Required]
        public int GameId { get; set; }  // Foreign key for the game

        [Required]
        public string? PlayerId { get; set; } // Foreign key for the player (User)

        [Required]
        public int Row { get; set; }  // Row coordinate of the move

        [Required]
        public int Column { get; set; }  // Column coordinate of the move

        public int MoveNumber { get; set; }  // The move's number in the game (for tracking order)

        // Relationships
        public virtual Game? Game { get; set; }  // Navigation property to Game
        public virtual ApplicationUser? Player { get; set; }  // Navigation property to ApplicationUser (Player)
    }
}
