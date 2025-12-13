using Casino_Api.Models;

namespace Casino_Api.Repositories.Interfaces;

public interface ITenantApiKeyRepository : IRepository<TenantApiKey>
{
    Task<TenantApiKey?> GetByApiKeyAsync(string apiKey);
    Task<TenantApiKey?> GetActiveApiKeyAsync();
    Task<bool> ValidateApiKeyAsync(string apiKey);
}
