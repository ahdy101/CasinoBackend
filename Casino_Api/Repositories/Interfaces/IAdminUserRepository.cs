using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    /// <summary>
 /// Repository interface for AdminUser operations
    /// </summary>
    public interface IAdminUserRepository : IRepository<AdminUser>
    {
      Task<AdminUser?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
    }
}
