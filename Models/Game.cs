using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Othello_API.Models
{
    public class Game
    {
        [Key]
        public int GameId { get; set; }

        [Required]
        public string GameStatus { get; set; } = "ongoing"; // "ongoing", "finished", etc.

        public string? Result { get; set; } // "Player1 Wins", "Draw", etc.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Player References
        [ForeignKey("Player1")]
        public string? Player1Id { get; set; }
        public virtual ApplicationUser? Player1 { get; set; }

        [ForeignKey("Player2")]
        public string? Player2Id { get; set; }
        public virtual ApplicationUser? Player2 { get; set; }

        // Add WinnerId (nullable)
        [ForeignKey("Winner")]
        public string? WinnerId { get; set; }
        public virtual ApplicationUser? Winner { get; set; }

        // Relationships
        public virtual List<UserGame> UserGames { get; set; } = new();
        public virtual List<Move> Moves { get; set; } = new();
    }
}
