using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Othello_API.Models;

public class LeaderBoard
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Player")]
    public string PlayerId { get; set; } = string.Empty;

    public int Wins { get; set; } = 0;

    public virtual required ApplicationUser Player { get; set; }
}
