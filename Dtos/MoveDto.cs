namespace Othello_API.Dtos
{
    public class MoveDto
    {
        public string? PlayerId { get; set; }  // Player's ID (for ApplicationUser)
        public int Row { get; set; }  // X-coordinate for the move
        public int Column { get; set; }  // Y-coordinate for the move
    }
}
