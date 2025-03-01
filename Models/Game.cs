using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Othello_API.Models;

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

    [JsonIgnore]
    public virtual ApplicationUser? Player1 { get; set; } // Make it nullable to avoid serialization issues

    [ForeignKey("Player2")]
    public required string Player2Id { get; set; }

    [JsonIgnore]
    public virtual ApplicationUser? Player2 { get; set; } // Make it nullable

    // Add WinnerId (nullable)
    [ForeignKey("Winner")]
    public string? WinnerId { get; set; }

    [JsonIgnore]
    public virtual ApplicationUser? Winner { get; set; }

    // Relationships
    [JsonIgnore]
    public virtual List<UserGame> UserGames { get; set; } = new();

    [JsonIgnore]
    public virtual List<Move> Moves { get; set; } = new();
}
