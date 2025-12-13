using Casino_Api.Data;
using Casino_Api.Factories.Interfaces;
using Casino_Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Casino_Api.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IRepositoryFactory _repositoryFactory;
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private IBlackjackGameRepository? _blackjackGames;
    private ITenantApiKeyRepository? _tenantApiKeys;

    public IUserRepository Users => _users ??= _repositoryFactory.CreateRepository<IUserRepository>();
    public IBlackjackGameRepository BlackjackGames => _blackjackGames ??= _repositoryFactory.CreateRepository<IBlackjackGameRepository>();
    public ITenantApiKeyRepository TenantApiKeys => _tenantApiKeys ??= _repositoryFactory.CreateRepository<ITenantApiKeyRepository>();

    public UnitOfWork(AppDbContext context, IRepositoryFactory repositoryFactory)
    {
        _context = context;
        _repositoryFactory = repositoryFactory;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
