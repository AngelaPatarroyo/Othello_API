

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string userId); 
}
