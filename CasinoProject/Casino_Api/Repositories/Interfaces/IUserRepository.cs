using Casino_Api.Models;

namespace Casino_Api.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<decimal> GetBalanceAsync(int userId);
    Task UpdateBalanceAsync(int userId, decimal newBalance);
}
