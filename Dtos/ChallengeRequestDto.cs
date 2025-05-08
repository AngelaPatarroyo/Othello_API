using System.Text.Json.Serialization;

namespace Othello_API.DTOs
{
    public class ChallengeRequestDto
    {
        [JsonPropertyName("player1Id")]
        public string Player1Id { get; set; } = string.Empty;

        [JsonPropertyName("player2Id")]
        public string Player2Id { get; set; } = string.Empty;
    }
}
