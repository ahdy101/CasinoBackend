# Casino API - Complete Architecture Overview

## 🎰 Design Patterns Implemented

The Casino API implements **three enterprise-grade design patterns** working together to create a maintainable, testable, and scalable architecture:

1. **Repository Pattern** - Data access abstraction
2. **Unit of Work Pattern** - Transaction management
3. **Factory Pattern** - Object creation and lifecycle management

---

## 📐 Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                      Controllers Layer                       │
│  (AuthController, WalletController, BlackjackController)    │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                      Services Layer                          │
│  (AuthService, WalletService, BlackjackEngine)              │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  Unit of Work Pattern                        │
│              (Transaction Coordinator)                       │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  Repository Pattern                          │
│      (UserRepository, BlackjackGameRepository, etc.)        │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                   Factory Pattern                            │
│    (DbContextFactory, RepositoryFactory)                    │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  Database (MySQL)                            │
│        (Users, BlackjackGames, TenantApiKeys, etc.)         │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 Request Flow Example

### User Login Request

```
1. HTTP POST /api/auth/login
   ↓
2. AuthController receives request
   ↓
3. Validates API key (RequireApiKeyAttribute)
   ↓
4. Calls AuthService.Login()
   ↓
5. AuthService uses IUnitOfWork
   ↓
6. UnitOfWork lazy-loads Users repository (via RepositoryFactory)
   ↓
7. UserRepository.GetByEmailAsync() queries database
   ↓
8. Password verified with BCrypt
   ↓
9. JWT token generated
   ↓
10. Response returned to client
```

### Transaction Flow (Add Funds)

```
1. HTTP POST /api/wallet/add-funds
   ↓
2. WalletController validates request
   ↓
3. Calls WalletService.AddFunds()
   ↓
4. UnitOfWork.BeginTransactionAsync()
   ↓
5. UserRepository updates balance
   ↓
6. UnitOfWork.CommitTransactionAsync()
   ↓
7. Success response with new balance
```

---

## 🏗️ Project Structure

```
Casino_Api/
│
├── Controllers/                      # API Endpoints
│   ├── AuthController.cs            # Authentication endpoints
│   ├── WalletController.cs          # Wallet management
│   └── BlackjackController.cs       # Blackjack game logic
│
├── Services/                         # Business Logic Layer
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IWalletService.cs
│   │   └── IBlackjackEngine.cs
│   └── Implementations/
│       ├── AuthService.cs           # User authentication & JWT
│       ├── WalletService.cs         # Transaction management
│       └── BlackjackEngine.cs       # Game logic & RNG
│
├── Repositories/                     # Data Access Layer
│   ├── Interfaces/
│   │   ├── IRepository.cs           # Generic CRUD
│   │   ├── IUserRepository.cs
│   │   ├── IBlackjackGameRepository.cs
│   │   ├── ITenantApiKeyRepository.cs
│   │   └── IUnitOfWork.cs          # Transaction coordinator
│   └── Implementations/
│       ├── Repository.cs            # Base implementation
│       ├── UserRepository.cs
│       ├── BlackjackGameRepository.cs
│       ├── TenantApiKeyRepository.cs
│       └── UnitOfWork.cs           # Manages transactions
│
├── Factories/                        # Object Creation
│   ├── Interfaces/
│   │   ├── IDbContextFactory.cs
│   │   └── IRepositoryFactory.cs
│   └── Implementations/
│       ├── DbContextFactory.cs      # Creates DbContext with config
│       └── RepositoryFactory.cs     # Creates repositories with caching
│
├── Infrastructure/                   # Core Utilities
│   ├── RandomNumberGenerator.cs     # Cryptographically secure RNG
│   └── CardDeckFactory.cs          # Card deck with Fisher-Yates shuffle
│
├── Security/                         # Authentication & Authorization
│   └── RequireApiKeyAttribute.cs    # Multi-tenant API key validation
│
├── Models/                           # Entity Models
│   ├── User.cs
│   ├── BlackjackGame.cs
│   ├── TenantApiKey.cs
│   ├── Card.cs
│   ├── Bet.cs
│   ├── PokerTable.cs
│   ├── GameHistory.cs
│   └── AdminUser.cs
│
├── DTOs/                             # Data Transfer Objects
│   ├── Requests/
│   │   ├── AuthRequests.cs
│   │   ├── BlackjackRequests.cs
│   │   ├── GameRequests.cs
│   │   ├── PokerRequests.cs
│   │   └── RouletteRequests.cs
│   └── Responses/
│       ├── AuthResponses.cs
│       ├── BlackjackResponses.cs
│       └── ErrorResponse.cs
│
├── Data/
│   └── AppDbContext.cs              # EF Core context with seeding
│
├── Program.cs                        # App startup & DI configuration
├── appsettings.json                 # Production configuration
├── appsettings.Development.json     # Development configuration
├── Casino_Api.csproj                # Project file with NuGet packages
├── README.md                         # Setup instructions
├── REPOSITORY_PATTERN.md            # Repository pattern documentation
└── FACTORY_PATTERN.md               # Factory pattern documentation
```

---

## 🎯 Design Pattern Interactions

### Pattern Integration Flow

```
Controller
    │
    ├─► Service (Business Logic)
    │       │
    │       ├─► Unit of Work (Transaction Management)
    │       │       │
    │       │       ├─► Repository Factory (Lazy Creation)
    │       │       │       │
    │       │       │       └─► Creates Repositories on-demand
    │       │       │
    │       │       └─► Manages DbContext lifecycle
    │       │
    │       └─► Infrastructure (RNG, Card Deck)
    │
    └─► Security Filters (API Key validation)
```

---

## 🔐 Security Architecture

### Multi-Layer Security

1. **API Key Validation** (Multi-tenancy)
   - Query string parameter
   - Database validation
   - Active key check

2. **JWT Authentication**
   - Bearer token
   - 120-minute expiry
   - User claims (ID, Username)

3. **Password Security**
   - BCrypt hashing
   - Auto-generated salts
   - Secure comparison

4. **Cryptographic RNG**
   - System.Security.Cryptography
   - Fair game outcomes
   - Fisher-Yates shuffle

---

## 📊 Database Design

### Entity Relationships

```
User (1) ──────< (Many) BlackjackGame
  │
  ├──────< (Many) Bet
  │
  └──────< (Many) GameHistory

TenantApiKey (standalone)
AdminUser (standalone)
PokerTable (standalone)
```

### Key Features

- ✅ Decimal(18,2) precision for currency
- ✅ Unique constraints (Email, Username, API Key)
- ✅ Cascade delete relationships
- ✅ Indexed columns for performance
- ✅ Seeded data (Admin user, Default API key)

---

## 🚀 Performance Features

### 1. Database Optimizations
- **Connection Pooling** - Managed by DbContextFactory
- **Retry Logic** - 3 retries with 5-second delay
- **Command Timeout** - 30 seconds
- **Lazy Loading Disabled** - Prevents N+1 queries

### 2. Repository Caching
- Repositories cached per request scope
- Lazy initialization (created only when needed)
- Memory-efficient

### 3. Transaction Management
- Explicit transaction control
- Automatic rollback on error
- Isolated transaction scope

### 4. Logging
- Production: Minimal logging
- Development: Detailed SQL queries
- Sensitive data logging (dev only)

---

## 🧪 Testing Strategy

### Unit Testing (Mockable Components)

```csharp
// Mock DbContext Factory
var mockDbFactory = new Mock<IDbContextFactory>();
mockDbFactory.Setup(x => x.CreateDbContext())
    .Returns(CreateInMemoryContext());

// Mock Repository Factory
var mockRepoFactory = new Mock<IRepositoryFactory>();
mockRepoFactory.Setup(x => x.CreateRepository<IUserRepository>())
    .Returns(mockUserRepo.Object);

// Mock Unit of Work
var mockUnitOfWork = new Mock<IUnitOfWork>();
mockUnitOfWork.Setup(x => x.Users.GetByEmailAsync(It.IsAny<string>()))
    .ReturnsAsync(testUser);

// Test Service
var authService = new AuthService(mockUnitOfWork.Object, mockConfig.Object);
var result = await authService.Login(loginRequest);
Assert.True(result.Success);
```

---

## 📦 Dependencies

### NuGet Packages

- **Microsoft.EntityFrameworkCore** 8.0.0 - ORM
- **Pomelo.EntityFrameworkCore.MySql** 8.0.0 - MySQL provider
- **Microsoft.AspNetCore.Authentication.JwtBearer** 8.0.0 - JWT auth
- **BCrypt.Net-Next** 4.0.3 - Password hashing
- **Swashbuckle.AspNetCore** 6.5.0 - Swagger/OpenAPI

---

## 🔄 Extending the Architecture

### Adding a New Game (e.g., Roulette)

1. **Create Model**
   ```csharp
   public class RouletteGame { ... }
   ```

2. **Create Repository Interface**
   ```csharp
   public interface IRouletteGameRepository : IRepository<RouletteGame> { ... }
   ```

3. **Implement Repository**
   ```csharp
   public class RouletteGameRepository : Repository<RouletteGame>, IRouletteGameRepository { ... }
   ```

4. **Update Repository Factory**
   ```csharp
   nameof(IRouletteGameRepository) => new RouletteGameRepository(_context)
   ```

5. **Add to Unit of Work**
   ```csharp
   public IRouletteGameRepository RouletteGames => _rouletteGames ??= ...
   ```

6. **Create Game Service**
   ```csharp
   public class RouletteEngine : IRouletteEngine { ... }
   ```

7. **Create Controller**
   ```csharp
   public class RouletteController : ControllerBase { ... }
   ```

8. **Register in DI**
   ```csharp
   builder.Services.AddScoped<IRouletteEngine, RouletteEngine>();
   ```

---

## 🎉 Architecture Benefits Summary

| Pattern | Benefit |
|---------|---------|
| **Repository** | Data access abstraction, testability |
| **Unit of Work** | Transaction management, consistency |
| **Factory** | Object creation control, lifecycle management |
| **Dependency Injection** | Loose coupling, testability |
| **Async/Await** | Scalability, non-blocking I/O |
| **JWT + API Key** | Secure multi-tenant authentication |
| **Crypto RNG** | Fair game outcomes |

---

## 📈 Current Status

✅ **Running** at `http://localhost:5000`  
✅ **Repository Pattern** - Data access abstraction  
✅ **Unit of Work** - Transaction management  
✅ **Factory Pattern** - Object creation  
✅ **JWT Authentication** - Secure tokens  
✅ **Multi-Tenancy** - API key validation  
✅ **Swagger Documentation** - Interactive API docs  
✅ **Production Ready** - Error handling, logging, security  

---

## 📚 Documentation Files

- **README.md** - Setup and getting started
- **REPOSITORY_PATTERN.md** - Repository pattern details
- **FACTORY_PATTERN.md** - Factory pattern details
- **ARCHITECTURE.md** - This file (complete overview)

---

**The Silver Slayed Casino API** - Enterprise-grade architecture for a luxury casino platform 🎰✨
