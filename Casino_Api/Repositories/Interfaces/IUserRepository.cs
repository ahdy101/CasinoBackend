using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for User operations
 /// </summary>
    public interface IUserRepository : IRepository<User>
    {
      Task<User?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task<decimal> GetBalanceAsync(int userId);
        Task UpdateBalanceAsync(int userId, decimal newBalance);
    }
}
