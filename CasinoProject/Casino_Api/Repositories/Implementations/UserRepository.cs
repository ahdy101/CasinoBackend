using Casino_Api.Data;
using Casino_Api.Models;
using Casino_Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Casino_Api.Repositories.Implementations;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<decimal> GetBalanceAsync(int userId)
    {
        var user = await _dbSet.FindAsync(userId);
        return user?.Balance ?? 0;
    }

    public async Task UpdateBalanceAsync(int userId, decimal newBalance)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.Balance = newBalance;
        }
    }
}
