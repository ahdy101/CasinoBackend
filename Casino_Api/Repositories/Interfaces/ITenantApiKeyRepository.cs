using Casino.Backend.Models;

namespace Casino.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for TenantApiKey operations
    /// </summary>
  public interface ITenantApiKeyRepository : IRepository<TenantApiKey>
    {
        Task<TenantApiKey?> GetByApiKeyAsync(string apiKey);
        Task<bool> ValidateApiKeyAsync(string apiKey);
    }
}
