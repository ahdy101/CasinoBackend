using Casino_Api.Data;
using Casino_Api.Repositories.Implementations;
using Casino_Api.Repositories.Interfaces;

namespace Casino_Api.Repositories.Factories;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories;

    public RepositoryFactory(AppDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public T CreateRepository<T>() where T : class
    {
        var type = typeof(T);

        if (_repositories.ContainsKey(type))
        {
            return (T)_repositories[type];
        }

        object repository = type.Name switch
        {
            nameof(IUserRepository) => new UserRepository(_context),
            nameof(IBlackjackGameRepository) => new BlackjackGameRepository(_context),
            nameof(ITenantApiKeyRepository) => new TenantApiKeyRepository(_context),
            _ => throw new NotSupportedException($"Repository type {type.Name} is not supported")
        };

        _repositories[type] = repository;
        return (T)repository;
    }
}
