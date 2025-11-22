# Casino Backend & Admin System

A full-stack casino web application built with ASP.NET Core Razor Pages and Web API.

## ?? Security Notice

**Before running this application, please read [SECURITY.md](SECURITY.md) for important security configuration steps.**

Sensitive configuration files (`appsettings.json`) are **not included** in this repository. You must create them from the provided example files.

## Projects

### Casino_Admin (Razor Pages Web App)
- User registration and login with cookie authentication
- Role-based access control (Admin/User)
- Games: Slots with randomized slot symbols
- User profile and balance management
- Admin dashboard to view users and balances
- Separate navigation for admins and users

### Casino_Api (Web API)
- RESTful API with Swagger documentation
- JWT authentication
- API key authorization (tenant-based)
- CRUD endpoints for:
  - Users
  - Admin Users
  - Tenant API Keys
  - Game (Roulette)

## Technologies

- **Framework**: .NET 10
- **Database**: MySQL (via Pomelo.EntityFrameworkCore.MySql)
- **Authentication**: Cookie-based (Razor Pages), JWT (API)
- **Authorization**: Role-based, API key (query parameter)
- **Password Hashing**: BCrypt.Net
- **API Documentation**: Swagger/OpenAPI

## Database Schema

### Tables
- `Users` - Normal users with username, password hash, balance, and role
- `AdminUsers` - Admin users (separate table)
- `Bets` - Game betting history
- `TenantApiKeys` - API keys for tenant authorization

**No EF Core migrations are used** - tables are created manually via SQL scripts.

## Setup

### Prerequisites
- .NET 10 SDK
- MySQL Server
- Visual Studio 2022 or VS Code

### Configuration

?? **IMPORTANT: Security First!**

1. **Copy example configuration files:**
```bash
# In Casino_Admin folder
copy appsettings.EXAMPLE.json appsettings.json

# In Casino_Api folder
copy appsettings.EXAMPLE.json appsettings.json
```

2. **Update connection strings** in both `appsettings.json` files with your MySQL credentials:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=casino_db;User=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;"
}
```

3. **Generate a strong JWT key** for `Casino_Api/appsettings.json`:
```powershell
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

4. **Create database and tables** (see SQL scripts in SECURITY.md)

5. **Insert an admin user and API key** in the database

### Database Setup

```sql
-- Create database
CREATE DATABASE casino_db;

-- Create tables (Users, AdminUsers, Bets, TenantApiKeys)
-- See SECURITY.md for complete SQL scripts

-- Insert admin user (password: Admin@123)
INSERT INTO AdminUsers (Username, PasswordHash, CreatedAt)
VALUES ('admin', '$2a$11$BCRYPT_HASH_HERE', NOW());

-- Insert API key
INSERT INTO TenantApiKeys (TenantName, ApiKey, CreatedAt)
VALUES ('default-tenant', 'YOUR_GENERATED_API_KEY', NOW());
```

### Running the Applications

**Run the Razor Pages App:**
```bash
cd Casino_Admin
dotnet run
```
Navigate to: `https://localhost:5001` (or the port shown in console)

**Run the API:**
```bash
cd Casino_Api
dotnet run
```
Access Swagger at: `https://localhost:PORT/swagger`

## Features

### User Features
- Register and login
- Play slots game with randomized symbols (??, ??, ??, 7??, ??, ??)
- Payouts: 3 of a kind (10x), 2 of a kind (2x), any 7 (1x)
- View profile and balance
- Logout

### Admin Features
- Login only (cannot register via UI)
- View all users and their balances
- Manage users via admin dashboard
- Access restricted to "Admin" role

### API Features
- Full CRUD for Users, AdminUsers, and TenantApiKeys
- Roulette game endpoint with JWT authentication
- API key authorization on all endpoints (`?apiKey=YOUR_KEY`)
- Swagger UI for testing
- All endpoints require valid API key

## Security Features

? **Implemented:**
- Passwords hashed with BCrypt
- Role-based authorization (Admin/User separation)
- API key validation per request
- Cookie-based authentication for web app (HttpOnly, Secure)
- JWT for API authentication
- No sensitive data in repository
- SQL parameterized queries (EF Core)
- HTTPS redirection
- Input validation

?? **Recommended for Production:**
- Rate limiting
- CORS policies
- Security headers (CSP, HSTS, etc.)
- Environment variables for secrets
- API key rotation
- Audit logging

See [SECURITY.md](SECURITY.md) for complete security configuration and best practices.

## Project Structure

```
CasinoBackend/
??? Casino_Admin/   # Razor Pages Web App
?   ??? Pages/
?   ?   ??? Account/        # Login, Register, Profile, Logout
?   ?   ??? Admin/        # Admin dashboard
?   ?   ??? Games/      # Games list, Slots, Details
?   ?   ??? Shared/   # Layout, navigation
?   ??? Models/             # User, Bet, AdminUser
?   ??? Data/             # AppDbContext
?   ??? appsettings.EXAMPLE.json
?
??? Casino_Api/          # Web API
?   ??? Controllers/ # Users, AdminUsers, TenantApiKeys, Game
?   ??? Models/        # User, Bet, AdminUser, TenantApiKey
?   ??? Data/    # AppDbContext
?   ??? Services/           # AuthService (JWT)
?   ??? Security/           # RequireApiKeyAttribute
?   ??? appsettings.EXAMPLE.json
?
??? .gitignore             # Excludes sensitive files
??? README.md            # This file
??? SECURITY.md# Security guide
```

## API Endpoints

All endpoints require `?apiKey=YOUR_KEY` query parameter.

### Users
- `GET /api/Users` - Get all users
- `GET /api/Users/{id}` - Get user by ID
- `POST /api/Users` - Create user
- `PUT /api/Users/{id}` - Update user
- `DELETE /api/Users/{id}` - Delete user

### Admin Users
- `GET /api/AdminUsers` - Get all admins
- `GET /api/AdminUsers/{id}` - Get admin by ID
- `POST /api/AdminUsers` - Create admin (password auto-hashed)
- `PUT /api/AdminUsers/{id}` - Update admin
- `DELETE /api/AdminUsers/{id}` - Delete admin

### Tenant API Keys
- `GET /api/TenantApiKeys` - Get all API keys
- `GET /api/TenantApiKeys/{id}` - Get API key by ID
- `POST /api/TenantApiKeys` - Create API key
- `PUT /api/TenantApiKeys/{id}` - Update API key
- `DELETE /api/TenantApiKeys/{id}` - Delete API key

### Game
- `POST /api/Game/roulette` - Play roulette (requires JWT + API key)

## Testing

### Test User Account
After registering via the web app, you can log in with your credentials.

### Test Admin Account
Insert an admin via SQL:
```sql
INSERT INTO AdminUsers (Username, PasswordHash, CreatedAt)
VALUES ('admin', 'BCRYPT_HASH_OF_Admin@123', NOW());
```
Then log in with:
- Username: `admin`
- Password: `Admin@123` (or your chosen password)

### Test API
1. Insert an API key in the database
2. Open Swagger UI
3. Add `?apiKey=YOUR_KEY` to requests
4. For `/api/Game/roulette`, also provide a JWT token from login

## Known Limitations

- No EF Core migrations (tables created manually)
- API key sent via query parameter (consider header for production)
- No rate limiting (recommended for production)
- No CORS configuration (add for production)
- Admin users stored in separate table (alternative: use roles column)

## Future Enhancements

- Add more casino games (Blackjack, Poker)
- Real-time game updates (SignalR)
- Transaction history
- Deposit/withdrawal system
- Multi-language support
- Progressive jackpots
- Leaderboards

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

**Security note:** Never commit `appsettings.json` or any file with real credentials.

## License

This project is for educational purposes only.

## Disclaimer

This application is a demonstration project and should not be used for real gambling or financial transactions without proper licensing, compliance, and security audits.

## Authors

Built with GitHub Copilot assistance.

## Support

For issues, please open a GitHub issue.  
For security vulnerabilities, see [SECURITY.md](SECURITY.md).
