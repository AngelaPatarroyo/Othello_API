using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Othello_API.Models;

public class Game
{
    [Key]
    public int GameId { get; set; }

    [Required]
    [StringLength(20)]
    public string GameStatus { get; set; } = "ongoing"; // "ongoing", "finished", etc.

    public string? Result { get; set; } // "Player1 Wins", "Draw", etc.

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Player References
    public required string Player1Id { get; set; }
    
    [JsonIgnore]
    public virtual ApplicationUser? Player1 { get; set; }

    public required string Player2Id { get; set; }
    
    [JsonIgnore]
    public virtual ApplicationUser? Player2 { get; set; }

    // Winner Reference (Nullable)
    public string? WinnerId { get; set; }
    
    [JsonIgnore]
    public virtual ApplicationUser? Winner { get; set; }

    // Relationships
    [JsonIgnore]
    public virtual List<UserGame> UserGames { get; set; } = new();

    [JsonIgnore]
    public virtual List<Move> Moves { get; set; } = new();
}
