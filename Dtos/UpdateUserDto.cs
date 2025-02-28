namespace Othello_API.Dtos
{
    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
    }
}
