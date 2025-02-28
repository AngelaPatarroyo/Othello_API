using System.Threading.Tasks;
using Othello_API.Models; 

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string userId); 
}
