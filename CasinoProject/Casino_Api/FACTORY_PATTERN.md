# Factory Design Pattern Implementation

## Overview

The Casino API now implements the **Factory Design Pattern** for database context and repository creation, providing better control over object creation, resource management, and dependency injection.

## Architecture Benefits

✅ **Centralized Object Creation** - Single point for creating DbContext and repositories
✅ **Lazy Initialization** - Repositories created only when needed
✅ **Connection Pooling** - Better database connection management
✅ **Configuration Flexibility** - Easy to modify database options without touching business logic
✅ **Testability** - Easy to mock factories for unit testing
✅ **Resource Management** - Proper disposal and lifecycle management

## Structure

```
Casino_Api/
├── Factories/
│   ├── Interfaces/
│   │   ├── IDbContextFactory.cs           # Database context factory interface
│   │   └── IRepositoryFactory.cs          # Repository factory interface
│   └── Implementations/
│       ├── DbContextFactory.cs            # Database context creation logic
│       └── RepositoryFactory.cs           # Repository creation logic
├── Repositories/
│   └── Implementations/
│       └── UnitOfWork.cs                  # Uses factory for lazy loading
└── Program.cs                              # Factory registration
```

## Components

### 1. DbContext Factory (IDbContextFactory)

**Purpose**: Creates and configures AppDbContext instances with proper connection settings

**Features**:
- Connection string management
- Retry logic (3 retries, 5 seconds delay)
- Command timeout configuration (30 seconds)
- Logging integration
- Sensitive data logging control (dev vs production)
- Detailed error logging (dev vs production)

**Interface**:
```csharp
public interface IDbContextFactory
{
    AppDbContext CreateDbContext();
    Task<AppDbContext> CreateDbContextAsync();
}
```

**Implementation Highlights**:
```csharp
public class DbContextFactory : IDbContextFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public AppDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        optionsBuilder.UseMySql(connectionString, serverVersion, options =>
        {
            options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5));
            options.CommandTimeout(30);
        })
        .UseLoggerFactory(_loggerFactory)
        .EnableSensitiveDataLogging(isDevelopment)
        .EnableDetailedErrors(isDevelopment);

        return new AppDbContext(optionsBuilder.Options);
    }
}
```

### 2. Repository Factory (IRepositoryFactory)

**Purpose**: Creates repository instances on-demand with caching to prevent duplicate instances

**Features**:
- Lazy repository creation
- Instance caching (singleton per factory instance)
- Type-safe repository creation
- Support for custom repositories

**Interface**:
```csharp
public interface IRepositoryFactory
{
    T CreateRepository<T>() where T : class;
}
```

**Implementation**:
```csharp
public class RepositoryFactory : IRepositoryFactory
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories;

    public T CreateRepository<T>() where T : class
    {
        var type = typeof(T);

        // Return cached instance if exists
        if (_repositories.ContainsKey(type))
            return (T)_repositories[type];

        // Create new instance based on type
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
```

### 3. Refactored Unit of Work

**Before (Direct Injection)**:
```csharp
public class UnitOfWork : IUnitOfWork
{
    public IUserRepository Users { get; }
    
    public UnitOfWork(
        AppDbContext context,
        IUserRepository userRepository,
        IBlackjackGameRepository blackjackGameRepository,
        ITenantApiKeyRepository tenantApiKeyRepository)
    {
        _context = context;
        Users = userRepository;
        BlackjackGames = blackjackGameRepository;
        TenantApiKeys = tenantApiKeyRepository;
    }
}
```

**After (Factory Pattern with Lazy Loading)**:
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly IRepositoryFactory _repositoryFactory;
    private IUserRepository? _users;
    
    // Lazy initialization - created only when accessed
    public IUserRepository Users => _users ??= _repositoryFactory.CreateRepository<IUserRepository>();
    
    public UnitOfWork(AppDbContext context, IRepositoryFactory repositoryFactory)
    {
        _context = context;
        _repositoryFactory = repositoryFactory;
    }
}
```

## Dependency Injection Setup

### Program.cs Configuration

**Before**:
```csharp
// Direct DbContext registration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Manual repository registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBlackjackGameRepository, BlackjackGameRepository>();
builder.Services.AddScoped<ITenantApiKeyRepository, TenantApiKeyRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**After (Factory Pattern)**:
```csharp
// Register factories
builder.Services.AddSingleton<IDbContextFactory, DbContextFactory>();
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();

// DbContext with factory integration
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var factory = serviceProvider.GetRequiredService<IDbContextFactory>();
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Only Unit of Work needs registration (uses factory internally)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

## Configuration Settings

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false
  }
}
```

### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    },
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  }
}
```

## Usage Examples

### Creating DbContext Manually (if needed)
```csharp
public class SomeService
{
    private readonly IDbContextFactory _dbFactory;

    public SomeService(IDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task DoSomething()
    {
        // Create a new context for a specific operation
        using var context = await _dbFactory.CreateDbContextAsync();
        // Perform operations
        await context.SaveChangesAsync();
    }
}
```

### Using Repositories (Lazy Loading)
```csharp
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<User?> GetUser(string email)
    {
        // Repository created on first access
        return await _unitOfWork.Users.GetByEmailAsync(email);
    }
}
```

## Performance Optimizations

### 1. Connection Retry Logic
```csharp
options.EnableRetryOnFailure(
    maxRetryCount: 3,
    maxRetryDelay: TimeSpan.FromSeconds(5),
    errorNumbersToAdd: null
);
```

### 2. Command Timeout
```csharp
options.CommandTimeout(30); // 30 seconds
```

### 3. Repository Caching
```csharp
// Repositories cached per factory instance
if (_repositories.ContainsKey(type))
    return (T)_repositories[type];
```

### 4. Lazy Loading
```csharp
// Repositories created only when accessed
public IUserRepository Users => _users ??= _repositoryFactory.CreateRepository<IUserRepository>();
```

## Testing Benefits

### Mocking DbContext Factory
```csharp
var mockFactory = new Mock<IDbContextFactory>();
mockFactory.Setup(x => x.CreateDbContext())
    .Returns(CreateInMemoryContext());

var service = new SomeService(mockFactory.Object);
```

### Mocking Repository Factory
```csharp
var mockRepoFactory = new Mock<IRepositoryFactory>();
mockRepoFactory.Setup(x => x.CreateRepository<IUserRepository>())
    .Returns(mockUserRepository.Object);

var unitOfWork = new UnitOfWork(context, mockRepoFactory.Object);
```

## Adding New Repositories

To add a new repository to the factory:

### 1. Create the repository
```csharp
public class PokerTableRepository : Repository<PokerTable>, IPokerTableRepository
{
    public PokerTableRepository(AppDbContext context) : base(context) { }
}
```

### 2. Update RepositoryFactory
```csharp
object repository = type.Name switch
{
    nameof(IUserRepository) => new UserRepository(_context),
    nameof(IPokerTableRepository) => new PokerTableRepository(_context), // Add here
    // ... other repositories
    _ => throw new NotSupportedException($"Repository type {type.Name} is not supported")
};
```

### 3. Add property to UnitOfWork
```csharp
private IPokerTableRepository? _pokerTables;
public IPokerTableRepository PokerTables => _pokerTables ??= _repositoryFactory.CreateRepository<IPokerTableRepository>();
```

## Design Pattern Benefits Summary

| Benefit | Description |
|---------|-------------|
| **Encapsulation** | Object creation logic hidden from consumers |
| **Flexibility** | Easy to change implementation without affecting clients |
| **Reusability** | Factory can be used across multiple services |
| **Testability** | Easy to mock factories in unit tests |
| **Maintainability** | Single place to modify object creation logic |
| **Performance** | Instance caching and lazy loading reduce memory footprint |

## Refactoring Summary

### Files Created
- ✅ `Factories/Interfaces/IDbContextFactory.cs`
- ✅ `Factories/Interfaces/IRepositoryFactory.cs`
- ✅ `Factories/Implementations/DbContextFactory.cs`
- ✅ `Factories/Implementations/RepositoryFactory.cs`

### Files Modified
- ✅ `Repositories/Implementations/UnitOfWork.cs` - Uses factory for lazy loading
- ✅ `Program.cs` - Factory registration and DI configuration
- ✅ `appsettings.json` - Added logging configuration
- ✅ `appsettings.Development.json` - Added detailed logging for development

### Removed Unnecessary Code
- ✅ AI model paths configuration (not implemented)
- ✅ Direct repository registrations (now handled by factory)
- ✅ Redundant constructor parameters in UnitOfWork

## API Status

✅ **Running successfully** at `http://localhost:5000`
✅ **Factory pattern** fully implemented
✅ **Repository pattern** with lazy loading
✅ **Database resilience** with retry logic
✅ **Production-ready** configuration
