using Casino_Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Casino_Api.Factories.Interfaces;

public interface IDbContextFactory
{
    AppDbContext CreateDbContext();
    Task<AppDbContext> CreateDbContextAsync();
}
