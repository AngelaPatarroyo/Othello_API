namespace Othello_API.Dtos
{
    public class MoveDto
    {
        public int MoveId { get; set; }  // Move ID (always present for responses)
        public int GameId { get; set; }  // Game ID
        public required string PlayerId { get; set; } // Player's ID (required)
        public int Row { get; set; }  // X-coordinate
        public int Column { get; set; }  // Y-coordinate
        public int MoveNumber { get; set; }  // Move order in the game
    }
}
