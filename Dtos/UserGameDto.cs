using System.ComponentModel.DataAnnotations;

namespace Othello_API.DTOs
{
    public class UserGameDto
    {
        [Required(ErrorMessage = "UserId is required.")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "GameId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GameId must be greater than 0.")]
        public int GameId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "TotalWins cannot be negative.")]
        public int? TotalWins { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "TotalLosses cannot be negative.")]
        public int? TotalLosses { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "TotalGames cannot be negative.")]
        public int? TotalGames { get; set; } = 0;
    }
}
