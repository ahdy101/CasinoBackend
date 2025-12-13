# Repository Facade Pattern Implementation

## Overview

The Casino API has been refactored to implement the **Repository Facade Pattern** with **Unit of Work**, providing a clean separation between data access and business logic layers.

## Architecture

### Pattern Benefits

✅ **Separation of Concerns** - Data access logic isolated from business logic
✅ **Testability** - Services can be unit tested with mocked repositories
✅ **Flexibility** - Easy to swap data sources without changing business logic
✅ **Transaction Management** - Centralized through Unit of Work
✅ **Reduced Duplication** - Common CRUD operations in base repository
✅ **Better Maintainability** - Clear dependency flow

## Structure

```
Casino_Api/
├── Repositories/
│   ├── Interfaces/
│   │   ├── IRepository.cs                    # Generic repository interface
│   │   ├── IUserRepository.cs                # User-specific operations
│   │   ├── IBlackjackGameRepository.cs       # Blackjack-specific operations
│   │   ├── ITenantApiKeyRepository.cs        # API key operations
│   │   └── IUnitOfWork.cs                    # Transaction coordinator
│   └── Implementations/
│       ├── Repository.cs                     # Generic repository implementation
│       ├── UserRepository.cs                 # User repository
│       ├── BlackjackGameRepository.cs        # Blackjack repository
│       ├── TenantApiKeyRepository.cs         # API key repository
│       └── UnitOfWork.cs                     # Unit of Work implementation
└── Services/
    └── Implementations/
        ├── AuthService.cs                    # Uses IUnitOfWork
        ├── WalletService.cs                  # Uses IUnitOfWork
        └── BlackjackEngine.cs                # Uses IUnitOfWork
```

## Components

### 1. Generic Repository (IRepository<T>)

**Purpose**: Base interface for common CRUD operations

**Methods**:
- `GetByIdAsync(int id)` - Fetch entity by ID
- `GetAllAsync()` - Fetch all entities
- `FindAsync(Expression<Func<T, bool>> predicate)` - Query with filter
- `FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)` - Get first match
- `AnyAsync(Expression<Func<T, bool>> predicate)` - Check existence
- `AddAsync(T entity)` - Add new entity
- `AddRangeAsync(IEnumerable<T> entities)` - Add multiple entities
- `Update(T entity)` - Update entity
- `Remove(T entity)` - Delete entity
- `RemoveRange(IEnumerable<T> entities)` - Delete multiple entities

### 2. Specialized Repositories

#### IUserRepository
```csharp
Task<User?> GetByEmailAsync(string email);
Task<User?> GetByUsernameAsync(string username);
Task<bool> EmailExistsAsync(string email);
Task<bool> UsernameExistsAsync(string username);
Task<decimal> GetBalanceAsync(int userId);
Task UpdateBalanceAsync(int userId, decimal newBalance);
```

#### IBlackjackGameRepository
```csharp
Task<IEnumerable<BlackjackGame>> GetGamesByUserIdAsync(int userId);
Task<BlackjackGame?> GetActiveGameAsync(int userId);
Task<IEnumerable<BlackjackGame>> GetCompletedGamesAsync(int userId, int limit = 10);
```

#### ITenantApiKeyRepository
```csharp
Task<TenantApiKey?> GetByApiKeyAsync(string apiKey);
Task<TenantApiKey?> GetActiveApiKeyAsync();
Task<bool> ValidateApiKeyAsync(string apiKey);
```

### 3. Unit of Work (IUnitOfWork)

**Purpose**: Coordinate transactions across multiple repositories

**Properties**:
- `IUserRepository Users` - Access user repository
- `IBlackjackGameRepository BlackjackGames` - Access blackjack repository
- `ITenantApiKeyRepository TenantApiKeys` - Access API key repository

**Methods**:
- `SaveChangesAsync()` - Persist all changes
- `BeginTransactionAsync()` - Start transaction
- `CommitTransactionAsync()` - Commit transaction
- `RollbackTransactionAsync()` - Rollback transaction
- `Dispose()` - Clean up resources

## Usage Examples

### Before (Direct DbContext Access)
```csharp
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> Login(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
```

### After (Repository Pattern)
```csharp
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<User?> Login(string email)
    {
        return await _unitOfWork.Users.GetByEmailAsync(email);
    }
}
```

### Transaction Example
```csharp
public async Task<(bool Success, decimal NewBalance, string Message)> AddFunds(int userId, decimal amount)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();
        
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return (false, 0, "User not found");

        user.Balance += amount;
        await _unitOfWork.CommitTransactionAsync();

        return (true, user.Balance, $"Successfully added {amount:C}");
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return (false, 0, $"Error adding funds: {ex.Message}");
    }
}
```

## Dependency Injection Setup

**Program.cs Registration**:
```csharp
// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBlackjackGameRepository, BlackjackGameRepository>();
builder.Services.AddScoped<ITenantApiKeyRepository, TenantApiKeyRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services (now depend on IUnitOfWork)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IBlackjackEngine, BlackjackEngine>();
```

## Updated Components

### Services Refactored
1. **AuthService** - Now uses `IUnitOfWork` instead of `AppDbContext`
2. **WalletService** - Transaction management through Unit of Work
3. **BlackjackEngine** - Repository pattern for game persistence

### Security Updated
- **RequireApiKeyAttribute** - Changed from `IAuthorizationFilter` to `IAsyncAuthorizationFilter`
- Uses `IUnitOfWork` instead of direct `AppDbContext`

## Testing Benefits

The repository pattern makes unit testing much easier:

```csharp
// Mock the repository
var mockUnitOfWork = new Mock<IUnitOfWork>();
mockUnitOfWork.Setup(x => x.Users.GetByEmailAsync(It.IsAny<string>()))
    .ReturnsAsync(new User { Id = 1, Email = "test@test.com" });

// Test the service
var authService = new AuthService(mockUnitOfWork.Object, mockConfig.Object);
var result = await authService.Login(new LoginRequest { Email = "test@test.com" });

// Assert result
Assert.True(result.Success);
```

## Performance Considerations

✅ **Async/Await** - All repository methods are async for better scalability
✅ **Expression Trees** - LINQ expressions enable efficient queries
✅ **Lazy Loading** - Disabled to avoid N+1 query problems
✅ **Transaction Scope** - Minimized for better concurrency

## Future Extensions

To add a new entity repository:

1. Create interface in `Repositories/Interfaces/I[Entity]Repository.cs`
2. Implement in `Repositories/Implementations/[Entity]Repository.cs`
3. Add property to `IUnitOfWork` interface
4. Initialize in `UnitOfWork` constructor
5. Register in `Program.cs` DI container

Example:
```csharp
// 1. Interface
public interface IPokerTableRepository : IRepository<PokerTable>
{
    Task<IEnumerable<PokerTable>> GetActiveTablesAsync();
}

// 2. Implementation
public class PokerTableRepository : Repository<PokerTable>, IPokerTableRepository
{
    public PokerTableRepository(AppDbContext context) : base(context) { }
    
    public async Task<IEnumerable<PokerTable>> GetActiveTablesAsync()
    {
        return await FindAsync(t => t.Status == "Active");
    }
}

// 3. Add to IUnitOfWork
IPokerTableRepository PokerTables { get; }

// 4. Initialize in UnitOfWork constructor
PokerTables = pokerTableRepository;

// 5. Register in Program.cs
builder.Services.AddScoped<IPokerTableRepository, PokerTableRepository>();
```

## Summary

The Casino API now follows enterprise-grade architecture with:
- ✅ Clean separation between data access and business logic
- ✅ Testable services through dependency injection
- ✅ Centralized transaction management
- ✅ Flexible and maintainable codebase
- ✅ Following SOLID principles

The API is **running successfully** at `http://localhost:5000` with the new repository pattern! 🎰✨
