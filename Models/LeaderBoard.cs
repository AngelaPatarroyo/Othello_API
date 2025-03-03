using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class LeaderBoard
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Player")]
    public string PlayerId { get; set; } = string.Empty;

    public int Wins { get; set; } = 0;

    // Navigation property for the Player
    public virtual required ApplicationUser Player { get; set; }  // No need for 'required' here, EF handles it.
}
