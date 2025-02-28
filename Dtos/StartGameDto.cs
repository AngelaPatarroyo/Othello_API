using System.ComponentModel.DataAnnotations;

namespace Othello_API.Dtos
{
    public class StartGameDto
    {
        [Required]
        public required string Player1Id { get; set; }

        [Required]
        public required string Player2Id { get; set; }
    }
}
