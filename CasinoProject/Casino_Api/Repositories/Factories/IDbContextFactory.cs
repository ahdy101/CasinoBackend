using Casino_Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Casino_Api.Repositories.Factories;

public interface IDbContextFactory
{
    AppDbContext CreateDbContext();
    Task<AppDbContext> CreateDbContextAsync();
}
