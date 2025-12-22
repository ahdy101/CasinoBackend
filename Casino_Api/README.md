# Casino Backend API

A .NET 8 Casino API with support for Blackjack, Roulette, and other casino games.

## ?? Setup Instructions

### Prerequisites
- .NET 8 SDK
- MySQL Server
- Visual Studio 2022 or VS Code

### Configuration Setup

**Important:** The `appsettings.json` file is not included in the repository for security reasons.

1. **Copy the example configuration:**
   ```bash
 cd Casino_Api
   cp appsettings.EXAMPLE.json appsettings.json
   ```

2. **Update `appsettings.json` with your settings:**
   - Replace `YOUR_DB_USER` with your MySQL username
   - Replace `YOUR_DB_PASSWORD` with your MySQL password
   - Replace `REPLACE_WITH_YOUR_STRONG_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG` with a strong JWT secret key (minimum 32 characters)

3. **Database Setup:**
   ```sql
   CREATE DATABASE casino_db;
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

## ?? Security

- Never commit `appsettings.json` to version control
- Always use `appsettings.EXAMPLE.json` as a template
- Keep your JWT secret keys and database passwords secure
- Use environment variables or Azure Key Vault for production

## ?? Project Structure

```
Casino_Api/
??? Controllers/     # API endpoints
??? Models/              # Domain models
??? Services/   # Business logic
??? Repositories/        # Data access layer
??? DTOs/          # Data transfer objects
??? Infrastructure/      # Cross-cutting concerns
??? Data/     # Database context
```

## ?? Features

- JWT Authentication
- Role-based Authorization
- Blackjack Game Engine
- Roulette Game Engine
- Wallet Management
- Admin Portal
- API Key Management

## ?? API Documentation

Once running, visit:
- Swagger UI: `http://localhost:5001/swagger`

## ?? Testing

Default seeded credentials (development only):
- Username: `admin`
- Password: `admin123`
- API Key: `default_tenant_api_key_12345`

## ?? License

[Add your license here]
