using Casino_Api.Data;
using Casino_Api.Repositories.Factories;
using Microsoft.EntityFrameworkCore;

namespace Casino_Api.Repositories.Factories;

public class DbContextFactory : IDbContextFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public DbContextFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    public AppDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        ConfigureOptions(optionsBuilder);

        return new AppDbContext(optionsBuilder.Options);
    }

    public async Task<AppDbContext> CreateDbContextAsync()
    {
        return await Task.FromResult(CreateDbContext());
    }

    private void ConfigureOptions(DbContextOptionsBuilder<AppDbContext> optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null
                );
                options.CommandTimeout(30);
            })
            .UseLoggerFactory(_loggerFactory)
            .EnableSensitiveDataLogging(_configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging", false))
            .EnableDetailedErrors(_configuration.GetValue<bool>("Logging:EnableDetailedErrors", false));
    }
}
