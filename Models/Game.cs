
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        public required string Player1Id { get; set; }

        [JsonIgnore] // Ensures no extra tracking data
        public virtual required ApplicationUser Player1 { get; set; }

        [ForeignKey("Player2")]
        public required string Player2Id { get; set; }

        [JsonIgnore] // Prevents unnecessary serialization tracking
        public virtual required ApplicationUser Player2 { get; set; }

        // Add WinnerId (nullable)
        [ForeignKey("Winner")]
        public string? WinnerId { get; set; }

        [JsonIgnore] // Prevents $id tracking for Winner
        public virtual ApplicationUser? Winner { get; set; }

        // Relationships
        [JsonIgnore] // Ignore relationships that might generate $id
        public virtual List<UserGame> UserGames { get; set; } = new();

        [JsonIgnore]
        public virtual List<Move> Moves { get; set; } = new();
    }
}
