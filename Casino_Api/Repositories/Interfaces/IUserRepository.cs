using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for User operations
 /// </summary>
    public interface IUserRepository : IRepository<User>
    {
      Task<User?> GetByUsernameAsync(string username);
       Task<User?> GetByEmailAsync(string email);
      Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<decimal> GetBalanceAsync(int userId);
      Task UpdateBalanceAsync(int userId, decimal newBalance);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<IEnumerable<User>> GetActiveUsersAsync(int minutesThreshold = 15);
      Task UpdateLastActivityAsync(int userId);
    }
}
