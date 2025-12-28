# The Silver Slayed Casino - Full Stack Application

A luxury full-stack casino web application with a React frontend and .NET 8 Web API backend, featuring multiple casino games with JWT authentication and real-time game state management.

## 🎰 Overview

This project consists of two main components:
- **CasinoProject**: React-based frontend with luxury silver/gold theme
- **Casino_Api**: .NET 8 Web API backend with JWT authentication

## 🚀 Projects

### CasinoProject (React Frontend)
A premium Vegas-style online casino web application built with React featuring:
- **Luxury Design**: Silver and gold themed UI with light/dark mode
- **Four Casino Games**:
  - 🎰 Slots: Classic slot machine with metallic reels
  - 🃏 Blackjack: Beat the dealer to 21
  - ♠️ Poker: Simplified Texas Hold'em
  - 🎡 Roulette: European roulette
- **User Features**:
  - Authentication with welcome bonus ($15,000)
  - Profile management
  - Wallet system
  - Game history tracking
  - Saved games
- **Admin Dashboard**: User management and statistics

### Casino_Api (.NET 8 Web API Backend)
RESTful API with comprehensive game engine support:
- **Authentication**: JWT-based authentication
- **Authorization**: API key validation (tenant-based)
- **Game Engines**:
  - Blackjack with hit/stand/double/split
  - Roulette with inside/outside bets
  - Poker (Texas Hold'em)
- **Features**:
  - Wallet management
  - Game state persistence
  - User management
  - Admin controls
  - Swagger/OpenAPI documentation

## 🛠️ Technologies

### Frontend (CasinoProject)
- **React 18**: UI framework
- **React Router**: Client-side routing
- **Context API**: State management (Auth, Theme, GameState)
- **Vite**: Build tool and dev server
- **CSS3**: Custom styling with themes

### Backend (Casino_Api)
- **Framework**: .NET 8 Web API
- **Database**: MySQL (via Pomelo.EntityFrameworkCore.MySql)
- **Authentication**: JWT (JSON Web Tokens)
- **Authorization**: API key validation per tenant
- **Password Hashing**: BCrypt.Net
- **API Documentation**: Swagger/OpenAPI
- **Patterns**: Repository pattern, Factory pattern, Unit of Work

## 📊 Database Schema

### Tables
- `Users` - User accounts with username, password hash, balance, role
- `AdminUsers` - Admin accounts with soft delete support
- `Bets` - Game betting history and outcomes
- `GameHistory` - Historical record of all games played
- `GameState` - Persistent game state for save/resume
- `GameStatistics` - Aggregated statistics per user
- `BlackjackGame` - Active blackjack game sessions
- `PokerTable` - Poker game state and players
- `TenantApiKeys` - API keys for tenant authorization with IsActive flag

## 🚀 Setup

### Prerequisites
- **Node.js** (v16 or higher) for frontend
- **.NET 8 SDK** for backend
- **MySQL Server** (8.0 or higher)
- **Visual Studio 2022** or **VS Code**

### Frontend Setup (CasinoProject)

1. **Navigate to frontend directory:**
```bash
cd CasinoProject
```

2. **Install dependencies:**
```bash
npm install
```

3. **Configure API endpoint** (if needed) in `src/config/api.js`:
```javascript
export const API_BASE_URL = 'https://localhost:7001';
```

4. **Start development server:**
```bash
npm run dev
```

5. **Open browser** to `http://localhost:5173`

### Backend Setup (Casino_Api)

1. **Navigate to backend directory:**
```bash
cd Casino_Api
```

2. **Copy example configuration:**
```bash
# Windows PowerShell
copy appsettings.EXAMPLE.json appsettings.json

# Linux/Mac
cp appsettings.EXAMPLE.json appsettings.json
```

3. **Update `appsettings.json`** with your settings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=casino_db;User=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_YOUR_STRONG_SECRET_KEY_AT_LEAST_32_CHARACTERS",
    "Issuer": "CasinoApi",
    "Audience": "CasinoClient"
  },
  "ApiKeys": {
    "DefaultKey": "default_tenant_api_key_12345"
  }
}
```

4. **Generate strong JWT key** (PowerShell):
```powershell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

5. **Create database:**
```sql
CREATE DATABASE casino_db;
```

6. **Run database migrations** (or create tables manually - see SECURITY.md)

7. **Insert initial admin user:**
```sql
INSERT INTO AdminUsers (Username, Email, PasswordHash, CreatedAt, IsDeleted)
VALUES ('admin', 'admin@casino.com', '$2a$11$...', NOW(), 0);
```

8. **Insert API key:**
```sql
INSERT INTO TenantApiKeys (TenantName, ApiKey, CreatedAt, IsActive)
VALUES ('default-tenant', 'default_tenant_api_key_12345', NOW(), 1);
```

9. **Run the API:**
```bash
dotnet run
```

10. **Access Swagger UI** at `https://localhost:7001/swagger`

## ✨ Features

### Frontend Features
- **User Authentication**:
  - Register with $15,000 welcome bonus
  - Login with JWT token storage
  - Profile management
  - Password reset
  
- **Game Features**:
  - Play four different casino games
  - Real-time balance updates
  - Save and resume games
  - View game history
  - Track win/loss statistics

- **User Interface**:
  - Light/Dark theme toggle
  - Responsive design (mobile/tablet/desktop)
  - Luxury silver/gold aesthetic
  - Smooth animations and transitions
  - Exit warnings for unsaved games

- **Admin Dashboard**:
  - View all users
  - Monitor game statistics
  - Manage user accounts
  - View system analytics

### Backend API Features
- **Authentication & Authorization**:
  - JWT token generation and validation
  - API key validation per request
  - Role-based access control (Admin/User)
  - Secure password hashing with BCrypt

- **Game Engines**:
  - **Blackjack**: Full game logic with splits, doubles, insurance
  - **Roulette**: European roulette with all bet types
  - **Poker**: Texas Hold'em with hand evaluation
  - **Slots**: Configurable paylines and symbols

- **Wallet Management**:
  - Deposit/withdrawal
  - Balance tracking
  - Transaction history
  - Bet validation

- **Game State**:
  - Save/resume game sessions
  - Persistent game state storage
  - Multi-game support per user

- **API Documentation**:
  - Swagger/OpenAPI UI
  - Interactive endpoint testing
  - Request/response schemas

## 🔒 Security Features

✅ **Implemented:**
- Passwords hashed with BCrypt (cost factor 11)
- JWT authentication with secure token generation
- API key validation per request
- Role-based authorization (Admin/User)
- Soft delete for admin users
- SQL parameterized queries (EF Core protection)
- HTTPS enforcement
- Input validation and sanitization
- Secure HTTP headers
- CORS configuration
- Request size limits
- Environment-specific configurations

⚠️ **Additional Recommendations for Production:**
- Rate limiting on API endpoints
- API key rotation mechanism
- Security headers (CSP, HSTS, X-Frame-Options)
- Environment variables for all secrets
- Azure Key Vault or similar for secret management
- Comprehensive audit logging
- DDoS protection
- Database connection pooling
- Regular security audits

**🔐 Important:** Never commit `appsettings.json`, `.env`, or any files containing real credentials to version control.

## 📁 Project Structure

```
CasinoBackend/
│
├── CasinoProject/                # React Frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── common/          # Reusable UI components
│   │   │   └── layout/          # Header, Footer, Sidebars
│   │   ├── pages/
│   │   │   ├── Auth/            # Login, Register
│   │   │   ├── Games/           # Blackjack, Slots, etc.
│   │   │   ├── Admin/           # Admin Dashboard
│   │   │   ├── Profile/         # User Profile
│   │   │   ├── Wallet/          # Wallet Management
│   │   │   └── ...
│   │   ├── context/             # React Context (Auth, Theme, GameState)
│   │   ├── config/              # API configuration
│   │   ├── constants/           # Constants and assets
│   │   └── styles/              # Global styles and themes
│   ├── public/                  # Static assets
│   ├── index.html
│   ├── package.json
│   ├── vite.config.js
│   └── README.md                # Frontend documentation
│
├── Casino_Api/                   # .NET 8 Backend API
│   ├── Controllers/
│   │   ├── AuthController.cs   # Authentication endpoints
│   │   ├── AdminController.cs  # Admin management
│   │   ├── BlackjackController.cs
│   │   ├── WalletController.cs
│   │   └── GameStateController.cs
│   ├── Models/
│   │   ├── User.cs
│   │   ├── AdminUser.cs
│   │   ├── BlackjackGame.cs
│   │   ├── GameState.cs
│   │   └── ...
│   ├── DTOs/
│   │   ├── Requests/            # Request DTOs
│   │   └── Responses/           # Response DTOs
│   ├── Services/
│   │   ├── Implementations/     # Service implementations
│   │   │   ├── AuthService.cs
│   │   │   ├── BlackjackEngine.cs
│   │   │   ├── WalletService.cs
│   │   │   └── ...
│   │   └── Interfaces/          # Service contracts
│   ├── Repositories/
│   │   ├── Implementations/     # Repository implementations
│   │   ├── Interfaces/          # Repository contracts
│   │   └── Factories/           # Factory pattern
│   ├── Data/
│   │   └── AppDbContext.cs     # EF Core DbContext
│   ├── Security/
│   │   ├── RequireApiKeyAttribute.cs
│   │   └── TokenValidator.cs
│   ├── Middleware/              # Custom middleware
│   ├── Migrations/              # Database migrations
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs               # Application entry point
│   ├── appsettings.EXAMPLE.json # Configuration template
│   ├── webapi.instructions.md   # Backend instructions
│   └── README.md                # Backend documentation
│
├── .gitignore                   # Git ignore rules
├── CasinoSolution.sln           # Visual Studio solution
└── README.md                    # This file (main documentation)
```

## 🔌 API Endpoints

All API endpoints require:
- **API Key**: Header `X-API-Key` or query parameter `?apiKey=YOUR_KEY`
- **JWT Token**: For authenticated endpoints (Authorization: Bearer TOKEN)

### Authentication
- `POST /api/auth/register` - Register new user (returns JWT)
- `POST /api/auth/login` - Login user (returns JWT)
- `POST /api/auth/token` - Generate JWT token
- `GET /api/auth/validate` - Validate JWT token

### Admin Management
- `GET /api/admin/users` - Get all users (Admin only)
- `GET /api/admin/statistics` - Get system statistics (Admin only)
- `PUT /api/admin/users/{id}` - Update user (Admin only)
- `DELETE /api/admin/users/{id}` - Delete user (Admin only)

### Wallet
- `GET /api/wallet/balance` - Get current balance
- `POST /api/wallet/deposit` - Deposit funds
- `POST /api/wallet/withdraw` - Withdraw funds
- `GET /api/wallet/transactions` - Get transaction history

### Blackjack
- `POST /api/blackjack/start` - Start new game
- `POST /api/blackjack/hit` - Hit (take card)
- `POST /api/blackjack/stand` - Stand (end turn)
- `POST /api/blackjack/double` - Double down
- `POST /api/blackjack/split` - Split hand
- `GET /api/blackjack/game/{id}` - Get game state

### Game State
- `POST /api/gamestate/save` - Save game state
- `GET /api/gamestate/list` - Get saved games
- `GET /api/gamestate/{id}` - Load game state
- `DELETE /api/gamestate/{id}` - Delete saved game

### User Management (Admin API)
- `GET /api/AdminUsers` - Get all admins
- `POST /api/AdminUsers` - Create admin
- `PUT /api/AdminUsers/{id}` - Update admin
- `DELETE /api/AdminUsers/{id}` - Soft delete admin

## 🎮 Testing

### Test Frontend

1. **Register a new user:**
   - Navigate to registration page
   - Create account with username/email/password
   - Receive $15,000 welcome bonus

2. **Play games:**
   - Choose a game from lobby
   - Place bets and play
   - Balance updates in real-time

3. **Admin features:**
   - Login as admin user
   - Access admin dashboard
   - View user statistics

### Test Backend API

1. **Via Swagger UI:**
   ```
   https://localhost:7001/swagger
   ```

2. **Register/Login:**
   ```bash
   POST /api/auth/register
   {
     "username": "testuser",
     "email": "test@example.com",
     "password": "Test@123"
   }
   ```

3. **Use JWT token:**
   - Copy token from login response
   - Click "Authorize" in Swagger
   - Enter: `Bearer YOUR_TOKEN`

4. **Test game endpoints:**
   ```bash
   POST /api/blackjack/start
   {
     "betAmount": 100
   }
   ```

### Test Credentials

**Default Admin** (insert via SQL):
- Username: `admin`
- Password: `admin123`
- Email: `admin@casino.com`

**API Key** (from appsettings.json):
- Key: `default_tenant_api_key_12345`

## 📝 Development Notes

### Architecture Patterns
- **Repository Pattern**: Data access abstraction
- **Factory Pattern**: Object creation for repositories and contexts
- **Unit of Work**: Transaction management
- **Dependency Injection**: Service registration and resolution
- **Context API**: React state management

### Code Organization
- **DTOs**: Request/Response separation from domain models
- **Service Layer**: Business logic isolation
- **Middleware**: Cross-cutting concerns (auth, logging, error handling)
- **Components**: Reusable UI elements

### Database Considerations
- Entity Framework Core for ORM
- MySQL with Pomelo provider
- Soft delete implementation for admin users
- Indexed columns for performance
- Transaction support for game operations

## ⚠️ Known Limitations

- No real-money transactions (educational project only)
- Game results use pseudo-random number generation
- No real-time multiplayer features
- Limited game variations
- No progressive jackpots
- API key sent in header (consider more advanced authentication for production)
- No comprehensive audit logging system
- Rate limiting not implemented

## 🚀 Future Enhancements

### Planned Features
- [ ] Additional casino games (Baccarat, Craps)
- [ ] Live dealer integration (SignalR)
- [ ] Progressive jackpots system
- [ ] Tournament mode
- [ ] Leaderboards and achievements
- [ ] Social features (friend lists, chat)
- [ ] Mobile app (React Native)
- [ ] Multi-language support (i18n)
- [ ] Advanced analytics dashboard
- [ ] Loyalty rewards program

### Technical Improvements
- [ ] Redis caching layer
- [ ] Message queue (RabbitMQ/Azure Service Bus)
- [ ] Microservices architecture
- [ ] Kubernetes deployment
- [ ] CI/CD pipeline
- [ ] Automated testing (unit, integration, e2e)
- [ ] Performance monitoring (Application Insights)
- [ ] Rate limiting and throttling

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Contribution Guidelines
- Follow existing code style and conventions
- Write meaningful commit messages
- Add tests for new features
- Update documentation as needed
- **Never commit sensitive data** (credentials, API keys, etc.)

## 📄 License

This project is for educational and demonstration purposes only. Not licensed for commercial use or real gambling operations.

## ⚠️ Disclaimer

**IMPORTANT:** This application is a demonstration/educational project only and should NOT be used for:
- Real gambling or wagering
- Financial transactions with real money
- Any commercial gambling operations

Real gambling applications require:
- Proper gambling licenses and regulatory compliance
- Financial institution partnerships
- Age verification systems
- Responsible gambling features
- Legal counsel and compliance audits
- Security audits and penetration testing

## 👨‍💻 Development Team

Built with the assistance of GitHub Copilot and modern development practices.

## 📞 Support & Contact

- **Issues**: Open a GitHub issue for bugs or feature requests
- **Security**: For security vulnerabilities, please review security guidelines
- **Documentation**: See individual README files in `CasinoProject/` and `Casino_Api/` for detailed component documentation

## 📚 Additional Documentation

- [Frontend README](CasinoProject/README.md) - React frontend documentation
- [Backend README](Casino_Api/README.md) - .NET API documentation  
- [Backend Instructions](Casino_Api/webapi.instructions.md) - Detailed API setup guide
- [Frontend Instructions](CasinoProject/webapi.instructions.md) - Frontend integration guide

---

**Last Updated**: December 2025  
**Version**: 1.0.0
