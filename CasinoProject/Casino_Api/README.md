# The Silver Slayed Casino API

Luxury online casino backend built with .NET 8.0, Entity Framework Core, MySQL, and JWT authentication.

## 🎰 Features

- **JWT Authentication** - Secure token-based authentication
- **Multi-Tenant Support** - API key validation for tenant isolation
- **Casino Games** - Blackjack, Poker, Roulette, Slots
- **Cryptographically Secure RNG** - Fair game outcomes using System.Security.Cryptography
- **Wallet System** - Add funds, cash out, bet management with transaction support
- **Swagger Documentation** - Interactive API documentation at root URL

## 🛠️ Tech Stack

- **.NET 8.0** - Modern C# with nullable reference types
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8.0** - ORM with MySQL provider
- **MySQL** - Relational database
- **JWT Bearer** - Authentication tokens
- **BCrypt.Net** - Password hashing with auto-generated salts
- **Swagger/OpenAPI** - API documentation with Swashbuckle

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL Server 8.0+](https://dev.mysql.com/downloads/mysql/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension

## 🚀 Getting Started

### 1. Clone and Navigate

```powershell
cd C:\Users\ahmad\source\repos\CasinoBackend\CasinoProject\Casino_Api
```

### 2. Configure Database Connection

Edit `appsettings.json` and update the MySQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=casino_db;User=root;Password=YOUR_PASSWORD;"
  }
}
```

### 3. Install Dependencies

```powershell
dotnet restore
```

### 4. Create Database

The database will be created automatically on first run in Development mode. Alternatively, use EF Core migrations:

```powershell
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

### 5. Run the API

```powershell
dotnet run
```

The API will start at:
- **HTTPS**: `https://localhost:7001`
- **HTTP**: `http://localhost:5001`
- **Swagger UI**: Navigate to the root URL

## 📚 API Documentation

Once running, open your browser to the root URL to see the interactive Swagger documentation.

### Authentication Flow

1. **Register** - `POST /api/auth/register`
   ```json
   {
     "username": "player1",
     "email": "player1@example.com",
     "password": "Password123!"
   }
   ```

2. **Login** - `POST /api/auth/login`
   ```json
   {
     "email": "player1@example.com",
     "password": "Password123!"
   }
   ```
   
   Response includes JWT token and API key:
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIs...",
     "apiKey": "default_tenant_api_key_12345",
     "user": {
       "id": 1,
       "username": "player1",
       "email": "player1@example.com",
       "balance": 1000.00
     }
   }
   ```

3. **Use Token** - Add to request headers:
   ```
   Authorization: Bearer {token}
   ```

4. **Use API Key** - Add to query string:
   ```
   ?apiKey=default_tenant_api_key_12345
   ```

### Default Credentials

The database is seeded with:

- **Admin User**
  - Username: `admin`
  - Password: `Admin@123`

- **Default API Key**
  - Key: `default_tenant_api_key_12345`

## 🎮 Game Endpoints

### Blackjack
- `POST /api/blackjack/deal` - Start new game
- `POST /api/blackjack/hit` - Draw a card
- `POST /api/blackjack/stand` - End turn
- `POST /api/blackjack/double-down` - Double bet and draw one card

### Wallet
- `GET /api/wallet/balance` - Get current balance
- `POST /api/wallet/add-funds` - Deposit funds
- `POST /api/wallet/cash-out` - Withdraw funds

## 🏗️ Project Structure

```
Casino_Api/
├── Controllers/           # API endpoints
│   ├── AuthController.cs
│   ├── BlackjackController.cs
│   └── WalletController.cs
├── Data/                 # Database context
│   └── AppDbContext.cs
├── DTOs/                 # Data transfer objects
│   ├── Requests/
│   └── Responses/
├── Infrastructure/       # Core utilities
│   ├── CardDeckFactory.cs
│   └── RandomNumberGenerator.cs
├── Models/              # Entity models
│   ├── User.cs
│   ├── BlackjackGame.cs
│   └── ...
├── Security/            # Authentication
│   └── RequireApiKeyAttribute.cs
├── Services/            # Business logic
│   ├── Interfaces/
│   └── Implementations/
├── appsettings.json     # Configuration
├── Program.cs           # Application entry
└── Casino_Api.csproj    # Project file
```

## 🔒 Security Features

- **Password Hashing** - BCrypt with auto-generated salts
- **JWT Tokens** - 120-minute expiry
- **API Key Validation** - Multi-tenant isolation
- **Cryptographic RNG** - Fair game outcomes
- **CORS Protection** - React frontend whitelist
- **SQL Injection Protection** - Parameterized queries via EF Core

## 🧪 Testing

### Using Swagger UI
1. Navigate to root URL
2. Click "Authorize" and enter API key
3. Register or login to get JWT token
4. Click "Authorize" again and enter: `Bearer {token}`
5. Try endpoints

### Using curl

```powershell
# Register
curl -X POST https://localhost:7001/api/auth/register?apiKey=default_tenant_api_key_12345 `
  -H "Content-Type: application/json" `
  -d '{\"username\":\"player1\",\"email\":\"player1@example.com\",\"password\":\"Password123!\"}'

# Login
curl -X POST https://localhost:7001/api/auth/login?apiKey=default_tenant_api_key_12345 `
  -H "Content-Type: application/json" `
  -d '{\"email\":\"player1@example.com\",\"password\":\"Password123!\"}'
```

## 🎲 Game Rules

### Blackjack
- Blackjack (21 with 2 cards) pays 2.5x (3:2)
- Win pays 2x (1:1)
- Push returns bet
- Dealer draws to 17
- Double down doubles bet, draws one card, then stands

## 🔧 Development

### Add New Game

1. Create model in `Models/`
2. Add DbSet to `AppDbContext.cs`
3. Create interface in `Services/Interfaces/`
4. Implement service in `Services/Implementations/`
5. Register in `Program.cs` DI container
6. Create controller in `Controllers/`

### Environment Variables

Set via `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=casino_dev;User=root;Password=dev;"
  },
  "Jwt": {
    "Key": "development-secret-key-32-characters-minimum",
    "Issuer": "casino-dev",
    "Audience": "casino-dev-users",
    "ExpireMinutes": 1440
  }
}
```

## 📝 Database Schema

### Users
- Id (int, PK)
- Username (string, unique)
- Email (string, unique)
- PasswordHash (string)
- Balance (decimal 18,2)
- CreatedAt (datetime)

### BlackjackGames
- Id (int, PK)
- UserId (int, FK)
- BetAmount (decimal 18,2)
- PlayerHandJson (string)
- DealerHandJson (string)
- PlayerTotal (int)
- DealerTotal (int)
- Status (string)
- Payout (decimal 18,2, nullable)
- CreatedAt (datetime)
- CompletedAt (datetime, nullable)

## 🚀 Deployment

### Production Checklist
- [ ] Update JWT secret key (256-bit minimum)
- [ ] Configure production database connection
- [ ] Enable HTTPS redirection
- [ ] Set up logging (Serilog recommended)
- [ ] Configure rate limiting
- [ ] Set up health checks
- [ ] Enable response compression
- [ ] Configure CORS for production domain
- [ ] Set up database backups
- [ ] Enable application insights

### Publish

```powershell
dotnet publish -c Release -o ./publish
```

## 🤝 Frontend Integration

This API is designed to work with the React frontend located at:
```
C:\Users\ahmad\source\repos\CasinoBackend\CasinoProject\
```

The API allows CORS from:
- `http://localhost:3000` (Create React App)
- `http://localhost:5173` (Vite)

## 📞 Support

For issues or questions, refer to:
- [API Integration Guide](../API_INTEGRATION_GUIDE.md)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [EF Core Documentation](https://docs.microsoft.com/ef/core/)

## 📄 License

Proprietary - The Silver Slayed Casino © 2024
