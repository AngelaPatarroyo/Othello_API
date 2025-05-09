public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string userId);
    Task UpdateAsync(ApplicationUser user);
}
