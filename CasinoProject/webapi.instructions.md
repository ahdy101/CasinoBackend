---
applyTo: "Casino_Api/*.*"
---
# CasinoBackend Web API Project Guide

## Overview
ASP.NET Core Web API application on .NET 10.0 providing RESTful endpoints for a multi-tenant casino gaming platform with Blackjack, Poker, Roulette game engines, and AI-powered reinforcement learning simulation capabilities.

## Key Technologies
- **Framework**: .NET 10.0, ASP.NET Core Web API, C# 13.0
- **Auth**: JWT Bearer + API Key Multi-Tenancy
- **Docs**: Swagger/OpenAPI 3.0
- **DB**: Entity Framework Core 9.0 + MySQL (Pomelo)
- **Security**: BCrypt.Net for password hashing
- **AI/ML**: DQN (Deep Q-Network) for Blackjack, CFR (Counterfactual Regret Minimization) for Poker

## Architecture

### Controllers
All controllers must follow the standard `ApiController` pattern:

```csharp
[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IGameEngineService _gameEngineService;

    public GameController(AppDbContext db, IGameEngineService gameEngineService)
    {
     _db = db;
      _gameEngineService = gameEngineService;
    }
}
```

**?? CRITICAL**: Controllers must never directly access repositories. All data access must go through services or the `AppDbContext` for simple CRUD operations.

### Dependency Injection
All service dependencies must be registered in `Program.cs` using the service collection:

```csharp
// Service Layer
builder.Services.AddScoped<IGameEngineService, GameEngineService>();
builder.Services.AddScoped<IBlackjackEngine, BlackjackEngine>();
builder.Services.AddScoped<IPokerEngine, PokerEngine>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAIModelService, AIModelService>();

// Infrastructure
builder.Services.AddSingleton<IRandomNumberGenerator, CryptoRNG>();
builder.Services.AddSingleton<ICardDeckFactory, StandardCardDeckFactory>();
```

**?? CRITICAL**: When adding new controllers with service dependencies, always ensure the service interface is bound to its implementation in `Program.cs`. Missing bindings will cause runtime dependency injection failures.

### Multi-Tenancy & API Key Validation

#### API Key Validation Pattern (CRITICAL)
All endpoints (except authentication endpoints) **must** validate the tenant API key to ensure multi-tenant data isolation:

```csharp
private bool IsApiKeyValid(string apiKey)
{
    return !string.IsNullOrEmpty(apiKey) && _db.TenantApiKeys.Any(k => k.ApiKey == apiKey);
}

[HttpGet]
public async Task<IActionResult> GetAll([FromQuery] string apiKey)
{
  if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
    // Implementation
}
```

**Alternative: Use `[RequireApiKey]` Attribute**
```csharp
[RequireApiKey]
[HttpGet]
public async Task<IActionResult> GetProtectedResource()
{
    // API key validation handled by attribute filter
}
```

#### Tenant Ownership Verification for CUD Operations (CRITICAL)
All **Create**, **Update**, and **Delete** methods **must** verify that:
1. The API key belongs to a valid tenant
2. The entity being acted upon belongs to the same tenant
3. The user performing the action has appropriate permissions

**Pattern for Update/Delete:**
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] UpdateRequest request, [FromQuery] string apiKey)
{
    if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
    
    var tenant = await _db.TenantApiKeys.FirstOrDefaultAsync(k => k.ApiKey == apiKey);
    if (tenant == null) return Unauthorized();

    var entity = await _db.Entities.FindAsync(id);
    if (entity == null || entity.TenantId != tenant.Id)
    {
 return NotFound(new { Message = "Entity not found or access denied." });
    }

    // Proceed with update
}
```

### Game Engine Layer Architecture

#### Blackjack Engine
```csharp
public interface IBlackjackEngine
{
    Task<GameState> InitializeGame(int userId, decimal betAmount);
 Task<GameState> Hit(int gameId, int userId);
    Task<GameState> Stand(int gameId, int userId);
    Task<GameState> DoubleDown(int gameId, int userId);
Task<GameState> Split(int gameId, int userId);
    Task<decimal> CalculatePayout(GameState state);
}
```

**Implementation Rules:**
- Use cryptographically secure RNG for card shuffling
- Implement standard Blackjack rules (dealer hits on soft 17)
- Track game state transitions in database for audit
- Calculate payouts: Blackjack = 1.5x, Win = 1x, Push = 0x, Loss = -1x

#### Poker Engine
```csharp
public interface IPokerEngine
{
    Task<PokerGame> InitializeTable(int tableId, GameType gameType, decimal buyIn);
    Task<PokerGame> DealCards(int tableId);
    Task<PokerGame> ProcessAction(int tableId, int userId, PlayerAction action, decimal? amount);
    Task<HandResult> EvaluateHand(List<Card> cards);
    Task<List<Player>> DetermineWinners(int tableId);
}
```

**Implementation Rules:**
- Support Texas Hold'em and Omaha variants
- Implement pot calculation with side pots
- Handle all-in scenarios correctly
- Use hand evaluation algorithm (7-card combinations)

#### Roulette Engine
```csharp
public interface IRouletteEngine
{
    Task<RouletteResult> Spin(int userId, List<RouletteBet> bets);
    int GenerateRandomNumber(); // 0-36 for European, 0-37 for American
    Task<decimal> CalculatePayout(RouletteBet bet, int winningNumber);
}
```

**Payout Rules:**
- Straight (single number): 35:1
- Split (2 numbers): 17:1
- Street (3 numbers): 11:1
- Corner (4 numbers): 8:1
- Red/Black, Even/Odd: 1:1
- Dozen/Column: 2:1

### AI Model Service Layer

#### DQN Blackjack Agent
```csharp
public interface IDQNBlackjackAgent
{
    Task<PlayerAction> PredictAction(BlackjackState state);
    Task TrainModel(List<Episode> episodes);
    Task<ModelMetrics> EvaluatePerformance();
    Task SaveModel(string modelPath);
    Task LoadModel(string modelPath);
}
```

**Training Rules:**
- Use epsilon-greedy exploration (? = 0.1)
- Replay buffer size: 10,000 episodes
- Batch size: 32
- Learning rate: 0.001
- Discount factor (?): 0.99

#### CFR Poker Agent
```csharp
public interface ICFRPokerAgent
{
    Task<StrategyProfile> ComputeNashEquilibrium(PokerGameTree tree);
    Task<PlayerAction> SampleAction(PokerState state, StrategyProfile strategy);
    Task<Regrets> UpdateRegrets(List<PokerAction> history, decimal payoff);
    Task<double> ComputeExploitability(StrategyProfile strategy);
}
```

### Key Patterns

#### Controller Actions with JWT Authorization
```csharp
[Authorize]
[HttpPost("blackjack/hit")]
[SwaggerOperation(Summary = "Hit action in Blackjack game")]
[ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Hit([FromBody] HitRequest request, [FromQuery] string apiKey)
{
    if (!IsApiKeyValid(apiKey)) return Unauthorized("Invalid or missing API key.");
    
    if (!ModelState.IsValid) 
     return BadRequest(new ErrorResponse { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    
    var gameState = await _blackjackEngine.Hit(request.GameId, userId);
    
    return Ok(new GameStateResponse { State = gameState });
}
```

#### Wallet Transaction Pattern (CRITICAL)
All wallet operations **must** be atomic and include balance validation:

```csharp
public async Task<TransactionResult> PlaceBet(int userId, decimal amount, string gameType)
{
    using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return TransactionResult.UserNotFound();
        
        if (user.Balance < amount) 
       return TransactionResult.InsufficientFunds();

        // Deduct bet amount
        user.Balance -= amount;
        
        // Record transaction
        var bet = new Bet
     {
          UserId = userId,
     Game = gameType,
 Amount = amount,
        Choice = "Pending",
       Payout = 0m,
            CreatedAt = DateTime.UtcNow
        };
        
        _db.Bets.Add(bet);
   await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        
      return TransactionResult.Success(bet.Id);
    }
 catch (Exception ex)
    {
    await transaction.RollbackAsync();
        // Log exception
        return TransactionResult.Error(ex.Message);
    }
}
```

#### Game State Validation
```csharp
private async Task<IActionResult> ValidateGameAccess(int gameId, int userId)
{
    var game = await _db.BlackjackGames
        .Include(g => g.PlayerHands)
   .FirstOrDefaultAsync(g => g.Id == gameId);

    if (game == null) 
        return NotFound(new { Message = "Game not found" });

    if (!game.PlayerHands.Any(h => h.UserId == userId))
        return Forbid("You are not a participant in this game");

    if (game.Status == GameStatus.Completed)
        return BadRequest(new { Message = "Game has already ended" });

    return null; // Validation passed
}
```

#### Random Number Generation (CRITICAL)
**?? NEVER use `System.Random` for casino games - it is not cryptographically secure.**

```csharp
// ? Correct - Cryptographically Secure
private int GetSecureRandomInt(int min, int max)
{
    var bytes = new byte[4];
    RandomNumberGenerator.Fill(bytes);
    int val = Math.Abs(BitConverter.ToInt32(bytes, 0));
    return min + (val % (max - min));
}

// ? Wrong - Predictable
private int GetRandomInt(int min, int max)
{
    var rand = new Random(); // NEVER USE
    return rand.Next(min, max);
}
```

#### Card Deck Shuffling (CRITICAL)
Implement Fisher-Yates shuffle with cryptographic RNG:

```csharp
public class CardDeck
{
    private List<Card> _cards;

    public void Shuffle()
    {
        int n = _cards.Count;
        while (n > 1)
     {
  n--;
        int k = GetSecureRandomInt(0, n + 1);
            var temp = _cards[k];
          _cards[k] = _cards[n];
        _cards[n] = temp;
        }
    }

    private int GetSecureRandomInt(int min, int max)
    {
        var bytes = new byte[4];
     RandomNumberGenerator.Fill(bytes);
        int val = Math.Abs(BitConverter.ToInt32(bytes, 0));
        return min + (val % (max - min));
    }
}
```

#### Password Hashing (CRITICAL)
**?? Always use BCrypt for password storage. Never store plain text passwords.**

```csharp
// Create User
public async Task<User> RegisterUser(string username, string password)
{
    // Hash password with BCrypt (auto-generates salt)
    var hash = BCrypt.Net.BCrypt.HashPassword(password);
    
    var user = new User
    {
        Username = username,
        PasswordHash = hash,
        Balance = 1000m, // Initial bonus
   CreatedAt = DateTime.UtcNow
    };
    
    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    return user;
}

// Update User - Preserve hash if password not changed
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
{
    var user = await _db.Users.FindAsync(id);
    if (user == null) return NotFound();

    user.Username = request.Username;
    
  // Only rehash if new password provided
    if (!string.IsNullOrEmpty(request.NewPassword))
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    }
    // Otherwise preserve existing hash
    
 await _db.SaveChangesAsync();
    return NoContent();
}
```

#### Circular Reference Prevention (CRITICAL)
Configure JSON serialization in `Program.cs` to prevent Swagger/API errors:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
 options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
```

**Navigation Property Pattern:**
```csharp
// Domain Model
public class Bet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } // Navigation property - can cause circular reference
    public decimal Amount { get; set; }
}

// Response DTO - Break circular reference
public class BetResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } // Flatten the relationship
    public decimal Amount { get; set; }
}
```

### Error Handling

#### Standard HTTP Status Codes
- **200 OK**: Successful GET/POST/PUT operation
- **201 Created**: Successful resource creation
- **204 No Content**: Successful DELETE or update with no response body
- **400 Bad Request**: Invalid input/validation errors
- **401 Unauthorized**: Missing or invalid authentication token/API key
- **403 Forbidden**: Valid authentication but insufficient permissions
- **404 Not Found**: Resource does not exist
- **409 Conflict**: Business rule violation (e.g., insufficient balance)
- **500 Internal Server Error**: Unhandled exception

#### Error Response Pattern
```csharp
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Usage in controller
if (user.Balance < betAmount)
{
    return Conflict(new ErrorResponse
    {
        Message = "Insufficient balance",
        Errors = new List<string> { $"Required: {betAmount}, Available: {user.Balance}" }
    });
}
```

#### ModelState Validation
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new ErrorResponse
 {
   Message = "Validation failed",
 Errors = ModelState.Values
                .SelectMany(v => v.Errors)
      .Select(e => e.ErrorMessage)
       .ToList()
      });
    }
    // Proceed with creation
}
```

### Swagger Documentation

#### Controller-Level Configuration
```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Game engine endpoints for Blackjack operations")]
public class BlackjackController : ControllerBase
{
    // ...
}
```

#### Action-Level Documentation
```csharp
[HttpPost("deal")]
[Authorize]
[SwaggerOperation(
    Summary = "Deal initial cards for new Blackjack game",
    Description = "Creates a new Blackjack game session, deducts bet from wallet, deals 2 cards to player and dealer",
    OperationId = "DealBlackjack",
    Tags = new[] { "Blackjack" }
)]
[ProducesResponseType(typeof(BlackjackGameResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
public async Task<IActionResult> DealCards([FromBody] DealRequest request)
{
    // Implementation
}
```

#### Request/Response Models Documentation
```csharp
/// <summary>
/// Request model for placing a Blackjack bet and dealing cards
/// </summary>
public class DealRequest
{
    /// <summary>
    /// Amount to bet on this hand (must be greater than 0 and less than user balance)
    /// </summary>
    [Required]
    [Range(1, 100000)]
    public decimal BetAmount { get; set; }

    /// <summary>
    /// Optional table ID for multi-player games
    /// </summary>
 public int? TableId { get; set; }
}
```

### Data Models & Entities

#### Domain Model Pattern
```csharp
public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
  
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0m;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Bet> Bets { get; set; }
    public ICollection<BlackjackGame> BlackjackGames { get; set; }
}
```

#### Request/Response DTO Pattern
```csharp
// Request DTO - Input validation
public class PlaceBetRequest
{
    [Required(ErrorMessage = "Game type is required")]
    public string GameType { get; set; }
    
    [Required]
    [Range(1, 10000, ErrorMessage = "Bet amount must be between 1 and 10,000")]
    public decimal Amount { get; set; }
    
    [Required]
    public string Choice { get; set; } // e.g., "Hit", "Stand", "Red", "Black"
}

// Response DTO - Clean API surface
public class BetResponse
{
    public int BetId { get; set; }
    public string GameType { get; set; }
    public decimal Amount { get; set; }
    public string Choice { get; set; }
    public decimal Payout { get; set; }
    public decimal NewBalance { get; set; }
    public DateTime PlacedAt { get; set; }
}
```

#### Game State Models
```csharp
public class BlackjackGameState
{
    public int GameId { get; set; }
 public List<Card> PlayerHand { get; set; }
    public List<Card> DealerHand { get; set; }
    public int PlayerTotal { get; set; }
    public int DealerTotal { get; set; }
    public GameStatus Status { get; set; } // Active, PlayerBust, DealerBust, PlayerWin, DealerWin, Push
    public decimal BetAmount { get; set; }
    public decimal? Payout { get; set; }
    public bool CanHit { get; set; }
    public bool CanStand { get; set; }
    public bool CanDoubleDown { get; set; }
    public bool CanSplit { get; set; }
}

public enum GameStatus
{
    Active,
    PlayerBust,
    DealerBust,
    PlayerBlackjack,
    PlayerWin,
    DealerWin,
    Push
}
```

### Database Context Configuration

#### AppDbContext Pattern
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Entity Sets
    public DbSet<User> Users { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }
 public DbSet<Bet> Bets { get; set; }
    public DbSet<TenantApiKey> TenantApiKeys { get; set; }
    public DbSet<BlackjackGame> BlackjackGames { get; set; }
    public DbSet<PokerTable> PokerTables { get; set; }
    public DbSet<GameHistory> GameHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraints
        modelBuilder.Entity<User>()
 .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<TenantApiKey>()
       .HasIndex(t => t.ApiKey)
    .IsUnique();

        // Relationships
        modelBuilder.Entity<Bet>()
            .HasOne(b => b.User)
   .WithMany(u => u.Bets)
     .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimal precision
        modelBuilder.Entity<User>()
.Property(u => u.Balance)
     .HasPrecision(18, 2);

        modelBuilder.Entity<Bet>()
  .Property(b => b.Amount)
          .HasPrecision(18, 2);

        // Seed data
  SeedAdminUsers(modelBuilder);
    }

  private void SeedAdminUsers(ModelBuilder modelBuilder)
    {
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        modelBuilder.Entity<AdminUser>().HasData(new AdminUser
        {
            Id = 1,
          Username = "admin",
PasswordHash = adminHash,
            Role = "SuperAdmin",
     CreatedAt = DateTime.UtcNow
        });
    }
}
```

### Service Layer Architecture

#### Service Interface Pattern
```csharp
public interface IGameEngineService
{
    Task<GameResult> PlayBlackjack(int userId, PlayerAction action, int? gameId = null, decimal? betAmount = null);
    Task<GameResult> PlayRoulette(int userId, List<RouletteBet> bets);
    Task<GameResult> PlayPoker(int tableId, int userId, PokerAction action, decimal? amount = null);
    Task<GameHistory> GetGameHistory(int userId, string gameType, DateTime? from = null, DateTime? to = null);
    Task<PlayerStats> GetPlayerStatistics(int userId);
}
```

#### Service Implementation Pattern
```csharp
public class GameEngineService : IGameEngineService
{
    private readonly AppDbContext _db;
    private readonly IBlackjackEngine _blackjackEngine;
    private readonly IRouletteEngine _rouletteEngine;
    private readonly IWalletService _walletService;
    private readonly ILogger<GameEngineService> _logger;

    public GameEngineService(
     AppDbContext db,
        IBlackjackEngine blackjackEngine,
      IRouletteEngine rouletteEngine,
        IWalletService walletService,
        ILogger<GameEngineService> logger)
    {
     _db = db;
 _blackjackEngine = blackjackEngine;
        _rouletteEngine = rouletteEngine;
   _walletService = walletService;
        _logger = logger;
    }

    public async Task<GameResult> PlayBlackjack(int userId, PlayerAction action, int? gameId = null, decimal? betAmount = null)
    {
        try
        {
  // Service orchestrates multiple operations
            if (gameId == null && betAmount.HasValue)
   {
     // New game - validate wallet and deduct bet
     var walletResult = await _walletService.DeductBet(userId, betAmount.Value);
        if (!walletResult.Success) return GameResult.InsufficientFunds();

// Initialize game
    var game = await _blackjackEngine.InitializeGame(userId, betAmount.Value);
  return GameResult.Success(game);
          }
     else if (gameId.HasValue)
      {
   // Continue existing game
  var game = await _blackjackEngine.ProcessAction(gameId.Value, userId, action);
          
   // If game ended, process payout
    if (game.Status != GameStatus.Active)
    {
          await _walletService.ProcessPayout(userId, game.BetAmount, game.Payout.GetValueOrDefault());
   }
    
    return GameResult.Success(game);
      }

  return GameResult.InvalidRequest();
        }
        catch (Exception ex)
   {
            _logger.LogError(ex, "Error in PlayBlackjack for user {UserId}", userId);
            return GameResult.Error(ex.Message);
        }
    }
}
```

### AI Model Integration

#### Model Endpoint Pattern
```csharp
[ApiController]
[Route("api/ai")]
public class AIModelController : ControllerBase
{
    private readonly IAIModelService _aiService;

    [HttpPost("blackjack/predict")]
    [Authorize]
    [SwaggerOperation(Summary = "Get AI-recommended action for Blackjack hand")]
    [ProducesResponseType(typeof(AIRecommendationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PredictBlackjackAction([FromBody] BlackjackStateRequest request)
    {
        var state = new BlackjackState
        {
            PlayerTotal = request.PlayerTotal,
   DealerUpCard = request.DealerUpCard,
IsSoftHand = request.IsSoftHand,
CanDoubleDown = request.CanDoubleDown,
     CanSplit = request.CanSplit
        };

 var action = await _aiService.PredictBlackjackAction(state);
        
        return Ok(new AIRecommendationResponse
     {
         RecommendedAction = action,
     Confidence = await _aiService.GetConfidence(state, action),
            ExpectedValue = await _aiService.GetExpectedValue(state, action)
        });
    }

    [HttpPost("poker/strategy")]
    [Authorize]
    [SwaggerOperation(Summary = "Get Nash equilibrium strategy for Poker situation")]
    public async Task<IActionResult> GetPokerStrategy([FromBody] PokerStateRequest request)
    {
        var strategy = await _aiService.ComputePokerStrategy(request.ToPokerState());
        return Ok(strategy);
  }
}
```

#### Training Endpoint Pattern (Admin Only)
```csharp
[HttpPost("train/blackjack")]
[Authorize(Roles = "SuperAdmin")]
[SwaggerOperation(Summary = "Train DQN model for Blackjack (Admin only)")]
[ProducesResponseType(typeof(TrainingResultResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> TrainBlackjackModel([FromBody] TrainingRequest request)
{
    var result = await _aiService.TrainDQNModel(
 episodes: request.Episodes,
        learningRate: request.LearningRate,
 batchSize: request.BatchSize
  );

    return Ok(new TrainingResultResponse
    {
        EpisodesTrained = result.Episodes,
        FinalLoss = result.Loss,
        AverageReward = result.AverageReward,
ModelPath = result.SavedModelPath,
        TrainingDuration = result.Duration
    });
}
```

### Folder Structure
```
Casino_Api/
??? Controllers/
?   ??? AdminUsersController.cs
?   ??? UsersController.cs
?   ??? TenantApiKeysController.cs
?   ??? GameController.cs
?   ??? BlackjackController.cs
?   ??? PokerController.cs
?   ??? RouletteController.cs
?   ??? AIModelController.cs
??? Models/
?   ??? User.cs
?   ??? AdminUser.cs
?   ??? Bet.cs
?   ??? TenantApiKey.cs
?   ??? BlackjackGame.cs
?   ??? PokerTable.cs
?   ??? Card.cs
?   ??? GameHistory.cs
??? DTOs/
?   ??? Requests/
?   ?   ??? PlaceBetRequest.cs
?   ?   ??? DealRequest.cs
?   ?   ??? PokerActionRequest.cs
?   ??? Responses/
?       ??? GameStateResponse.cs
?       ??? BetResponse.cs
?     ??? ErrorResponse.cs
??? Services/
?   ??? Interfaces/
?   ?   ??? IGameEngineService.cs
?   ?   ??? IBlackjackEngine.cs
?   ?   ??? IPokerEngine.cs
?   ? ??? IRouletteEngine.cs
?   ?   ??? IWalletService.cs
?   ?   ??? IAuthService.cs
?   ?   ??? IAIModelService.cs
?   ??? Implementations/
?       ??? GameEngineService.cs
?       ??? BlackjackEngine.cs
?       ??? PokerEngine.cs
?       ??? RouletteEngine.cs
? ??? WalletService.cs
?       ??? AuthService.cs
?    ??? AIModelService.cs
??? AI/
?   ??? DQN/
?   ?   ??? BlackjackAgent.cs
?   ?   ??? ReplayBuffer.cs
?   ?   ??? NeuralNetwork.cs
?   ??? CFR/
?       ??? PokerAgent.cs
?    ??? GameTree.cs
?       ??? StrategyProfile.cs
??? Security/
?   ??? RequireApiKeyAttribute.cs
?   ??? JwtTokenHelper.cs
??? Data/
?   ??? AppDbContext.cs
?   ??? Migrations/
??? Infrastructure/
?   ??? RandomNumberGenerator.cs
?   ??? CardDeckFactory.cs
?   ??? HandEvaluator.cs
??? Program.cs
```

### Best Practices

#### Controllers
1. Inherit from `ControllerBase` (not `Controller` for APIs)
2. Use service interfaces for all business logic
3. Apply `[Authorize]` for protected endpoints
4. Apply `[RequireApiKey]` or validate API keys manually for multi-tenant isolation
5. Include comprehensive Swagger documentation with `[SwaggerOperation]`
6. Use proper HTTP status codes (200, 201, 400, 401, 403, 404, 409, 500)
7. Return typed responses with `ProducesResponseType` attributes
8. Implement async/await patterns for all I/O operations
9. **Always validate tenant ownership for CUD operations**
10. **Never expose domain models directly - use DTOs**

#### Security Rules (CRITICAL)
1. **Password Hashing**: Always use BCrypt.Net with auto-generated salts
2. **API Keys**: Validate on every request except authentication endpoints
3. **JWT Tokens**: Include user ID and username in claims
4. **Multi-Tenancy**: Verify tenant ownership before data access
5. **Random Numbers**: Use `RandomNumberGenerator.Fill()` for all game RNG
6. **Input Validation**: Use Data Annotations and ModelState validation
7. **SQL Injection**: Use parameterized queries (EF Core handles this)
8. **Circular References**: Configure `ReferenceHandler.IgnoreCycles`

#### Game Engine Rules
1. **State Persistence**: Save all game states to database for audit
2. **Transaction Atomicity**: Use database transactions for wallet + game state changes
3. **Fairness**: Use cryptographically secure RNG for all random events
4. **Payout Calculation**: Double-check math for all game payouts
5. **Game History**: Record every action for dispute resolution
6. **AI Integration**: Separate AI endpoints from actual gameplay

#### Data Handling
- **Explicit Mapping**: Domain models ? DTOs (never expose entities directly)
- **Decimal Precision**: Use `decimal(18,2)` for all currency amounts
- **Null Safety**: Enable nullable reference types (`<Nullable>enable</Nullable>`)
- **Enum Handling**: Convert to strings for API consistency
- **DateTime**: Always use `DateTime.UtcNow` for consistency

#### Testing (Recommended)
- **Framework**: xUnit or NUnit
- **Mocking**: Moq for service/repository mocking
- **Pattern**: Arrange-Act-Assert
- **Coverage**: Unit tests for game logic, integration tests for API endpoints
- **AI Testing**: Separate test suite for model performance evaluation

### Performance Considerations
1. **Database Queries**: Use `.AsNoTracking()` for read-only queries
2. **Eager Loading**: Use `.Include()` to prevent N+1 queries
3. **Caching**: Implement caching for game rules, payout tables, AI models
4. **Connection Pooling**: Rely on EF Core's built-in connection pooling
5. **Async Operations**: Use async/await for all database and external calls

### Logging
```csharp
public class GameEngineService : IGameEngineService
{
    private readonly ILogger<GameEngineService> _logger;

    public async Task<GameResult> PlayBlackjack(int userId, PlayerAction action, int? gameId, decimal? betAmount)
 {
        _logger.LogInformation("User {UserId} playing Blackjack - Action: {Action}, GameId: {GameId}, Bet: {Bet}", 
       userId, action, gameId, betAmount);

        try
        {
  // Game logic
    }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error in PlayBlackjack for user {UserId}", userId);
    throw;
        }
    }
}
```

### Configuration (appsettings.json)
```json
{
"ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=casino_db;user=root;password=yourpassword"
  },
  "Jwt": {
    "Key": "your-super-secret-jwt-key-min-32-characters",
    "Issuer": "CasinoBackend",
    "Audience": "CasinoClients",
    "ExpireMinutes": 120
  },
  "GameSettings": {
    "BlackjackPayoutMultiplier": 1.5,
  "MinBet": 1,
    "MaxBet": 10000,
    "InitialBalance": 1000
  },
  "AISettings": {
    "DQNModelPath": "./Models/blackjack_dqn.h5",
    "CFRModelPath": "./Models/poker_cfr.json",
    "EnableAIRecommendations": true
  }
}
```

## Related Projects
- **Casino_Admin**: Razor Pages admin panel for user management
- **AI Models**: Python-based training scripts (separate repository)
- **Mobile Client**: Future React Native/Flutter app

This guide establishes strict conventions for building a secure, fair, and maintainable casino gaming platform with AI capabilities.
