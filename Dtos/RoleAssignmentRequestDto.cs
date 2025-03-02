namespace Othello_API.Dtos
{
    public class RoleAssignmentRequestDto
    {
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
