using System.ComponentModel.DataAnnotations;

namespace Othello_API.Dtos
{
    public class MoveDto
    {
        public int? MoveId { get; set; }  // Nullable because it's generated on the server

        [Required(ErrorMessage = "GameId is required.")]
        public int GameId { get; set; }  // Game ID

        [Required(ErrorMessage = "PlayerId is required.")]
        public required string PlayerId { get; set; } // Player's ID (required)

        [Required(ErrorMessage = "Row is required.")]
        [Range(0, 7, ErrorMessage = "Row must be between 0 and 7.")]
        public int Row { get; set; }  // X-coordinate

        [Required(ErrorMessage = "Column is required.")]
        [Range(0, 7, ErrorMessage = "Column must be between 0 and 7.")]
        public int Column { get; set; }  // Y-coordinate

        [Range(0, int.MaxValue, ErrorMessage = "Move number must be a positive value.")]
        public int MoveNumber { get; set; } = 0;  // Move order in the game (default 0)
    }
}
