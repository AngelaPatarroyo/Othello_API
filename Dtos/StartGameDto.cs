using System.ComponentModel.DataAnnotations;

namespace Othello_API.DTOs
{
    public class StartGameDto : IValidatableObject
    {
        [Required(ErrorMessage = "Player1Id is required.")]
        public string Player1Id { get; set; } = string.Empty; 
        [Required(ErrorMessage = "Player2Id is required.")]
        public string Player2Id { get; set; } = string.Empty; 

        [Required]
        [RegularExpression("ongoing|finished|cancelled", ErrorMessage = "GameStatus must be 'ongoing', 'finished', or 'cancelled'.")]
        public string GameStatus { get; set; } = "ongoing";

        public string? WinnerId { get; set; }
        public string? Result { get; set; }  

       
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Player1Id) && Player1Id == Player2Id)
            {
                yield return new ValidationResult("Player1Id and Player2Id must be different.", new[] { nameof(Player2Id) });
            }
        }
    }
}
