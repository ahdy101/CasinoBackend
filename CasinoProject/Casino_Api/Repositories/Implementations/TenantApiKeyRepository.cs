using Casino_Api.Data;
using Casino_Api.Models;
using Casino_Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Casino_Api.Repositories.Implementations;

public class TenantApiKeyRepository : Repository<TenantApiKey>, ITenantApiKeyRepository
{
    public TenantApiKeyRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<TenantApiKey?> GetByApiKeyAsync(string apiKey)
    {
        return await _dbSet.FirstOrDefaultAsync(k => k.ApiKey == apiKey);
    }

    public async Task<TenantApiKey?> GetActiveApiKeyAsync()
    {
        return await _dbSet.FirstOrDefaultAsync(k => k.IsActive);
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        return await _dbSet.AnyAsync(k => k.ApiKey == apiKey && k.IsActive);
    }
}
