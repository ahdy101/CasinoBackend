# Casino Backend API

A modern .NET 8 Casino API with JWT authentication, user management, and KYC verification system.

## ?? Setup Instructions

### Prerequisites
- .NET 8 SDK
- MySQL Server 8.0+
- Visual Studio 2022 or VS Code

### Configuration Setup

**Important:** The `appsettings.json` file is not included in the repository for security reasons.

1. **Copy the example configuration:**
   ```bash
   cd Casino_Api
cp appsettings.EXAMPLE.json appsettings.json
   ```

2. **Update `appsettings.json` with your settings:**
   - Replace `YOUR_DB_USER` with your MySQL username (default: `casino_user`)
   - Replace `YOUR_DB_PASSWORD` with your MySQL password
   - Replace JWT secret key with a strong key (minimum 32 characters)

3. **Database Setup:**
   ```sql
   CREATE DATABASE casino_db;
   CREATE USER 'casino_user'@'localhost' IDENTIFIED BY 'YOUR_PASSWORD';
   GRANT ALL PRIVILEGES ON casino_db.* TO 'casino_user'@'localhost';
   FLUSH PRIVILEGES;
   ```

4. **Run Database Migrations:**
   ```bash
   # Run all SQL migration files in order:
   mysql -u casino_user -p casino_db < Migrations/SQL/20250104_AddTenantId.sql
   mysql -u casino_user -p casino_db < Migrations/SQL/20250104_CreateKycTables.sql
   mysql -u casino_user -p casino_db < Migrations/SQL/20250104_CreatePasswordResetTokensTable.sql
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

## ?? Security

- Never commit `appsettings.json` to version control
- Always use `appsettings.EXAMPLE.json` as a template
- Keep your JWT secret keys and database passwords secure
- Use environment variables or Azure Key Vault for production
- All passwords are hashed using BCrypt
- JWT tokens expire after 120 minutes (configurable)

## ?? Project Structure

```
Casino_Api/
??? Controllers/        # API endpoints (Auth, Admin)
??? Models/        # Domain models (User, KYC, PasswordResetToken)
??? Services/   # Business logic layer
?   ??? Interfaces/     # Service contracts
?   ??? Implementations/# Service implementations
??? Repositories/       # Data access layer (Dapper-based)
?   ??? Interfaces/     # Repository contracts
?   ??? Implementations/# Repository implementations
??? DTOs/         # Data transfer objects
?   ??? Requests/       # API request models
?   ??? Responses/      # API response models
??? Infrastructure/     # Cross-cutting concerns (RNG, Card Deck)
??? Data/     # Database connection factory
??? Security/   # Security attributes (API Key validation)
??? Migrations/SQL/     # Database migration scripts
```

## ? Features

### Authentication & Authorization
- ? JWT Authentication (Bearer tokens)
- ? User Registration with automatic login
- ? Login with username/password
- ? Password Reset Flow (forgot password + reset with token)
- ? Profile Management (update username/email)
- ? Change Password (requires current password)
- ? WhoAmI endpoint (get current user info)
- ? Role-based Authorization (Player, Admin)

### User Management
- ? Soft Delete (users marked as deleted, not removed)
- ? Audit Trail (ModifiedAt, DeletedAt timestamps)
- ? Email Validation
- ? Username Uniqueness Check
- ? Secure Password Requirements (8+ chars, uppercase, lowercase, number, special char)

### KYC (Know Your Customer)
- ? KYC Status Tracking (NotStarted, Pending, UnderReview, Verified, Rejected, Expired)
- ? Separate PII Storage (KycDetails table with encryption support)
- ? Document Management (metadata only, files stored in cloud)
- ? Audit Logging (track admin access to KYC data)
- ? GDPR Compliance Ready

### Security Features
- ? API Key Validation (for game endpoints)
- ? Multi-Tenancy Support (TenantId field)
- ? Password Reset Tokens (one-time use, 1-hour expiry)
- ? CORS Configuration (frontend integration ready)
- ? BCrypt Password Hashing

### Technical Features
- ? Dapper ORM (lightweight, fast data access)
- ? Repository Pattern
- ? Service Layer Architecture
- ? DTO Pattern (separation of concerns)
- ? Structured Logging
- ? Swagger/OpenAPI Documentation
- ? ETag Support (for optimistic concurrency - see below)

## ?? ETag Validation for Updates/Deletes

**ETag (Entity Tag)** is used for optimistic concurrency control to prevent lost updates.

### How It Works:

```
1. Client GETs resource ? Server returns data + ETag header
2. Client modifies data
3. Client PUTs/DELETEs with If-Match: ETag header
4. Server validates: Current ETag == Provided ETag?
   - Match ? Update succeeds ?
   - No Match ? 412 Precondition Failed ? (someone else modified it)
```

### Implementation Example:

```csharp
// GET endpoint - returns ETag
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var user = await _userRepository.GetByIdAsync(id);
    var etag = Convert.ToBase64String(
        SHA256.HashData(Encoding.UTF8.GetBytes($"{user.Id}{user.ModifiedAt:O}"))
    );
    Response.Headers.Add("ETag", etag);
    return Ok(user);
}

// PUT endpoint - validates ETag
[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
{
  var ifMatch = Request.Headers["If-Match"].FirstOrDefault();
    if (string.IsNullOrEmpty(ifMatch))
     return BadRequest("If-Match header required");

    var user = await _userRepository.GetByIdAsync(id);
    var currentEtag = Convert.ToBase64String(
        SHA256.HashData(Encoding.UTF8.GetBytes($"{user.Id}{user.ModifiedAt:O}"))
    );

    if (ifMatch != currentEtag)
        return StatusCode(412, "Resource was modified by another user");

    // Proceed with update...
}
```

### Benefits:
- ? Prevents lost updates (concurrent modification detection)
- ? No database locks needed
- ? Improves API reliability in multi-user scenarios
- ? RESTful best practice

## ?? API Documentation

### Base URL
- Development: `https://localhost:44331`
- Swagger UI: `https://localhost:44331/swagger`

### Authentication Endpoints (Public)

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/register` | POST | ? No | Register new user |
| `/api/auth/login` | POST | ? No | Login and get JWT token |
| `/api/auth/forgot-password` | POST | ? No | Request password reset |
| `/api/auth/reset-password` | POST | ? No | Reset password with token |

### User Management Endpoints (Requires JWT)

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/whoami` | GET | ? JWT | Get current user info |
| `/api/auth/profile` | PUT | ? JWT | Update username/email |
| `/api/auth/change-password` | POST | ? JWT | Change password |

### Request Examples

**Register:**
```json
POST /api/auth/register
{
  "username": "player123",
  "email": "player@example.com",
  "password": "SecurePass123!"
}
```

**Login:**
```json
POST /api/auth/login
{
  "username": "player123",
  "password": "SecurePass123!"
}
```

**Update Profile (with JWT):**
```http
PUT /api/auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "username": "newusername",
  "email": "newemail@example.com"
}
```

## ?? Game Endpoints (Coming Soon)

Game controllers have been removed to start fresh. Future game endpoints will require:
- JWT Token (Bearer authentication)
- API Key (Tenant validation)
- ETag validation (for state updates)

## ??? Database Schema

### Core Tables
- `Users` - User accounts with authentication
- `TenantApiKeys` - API keys for multi-tenancy
- `PasswordResetTokens` - Secure password reset tokens

### KYC Tables
- `KycDetails` - Sensitive user PII (encrypted at rest)
- `KycDocuments` - Document metadata (files in cloud storage)
- `KycAuditLogs` - Admin access audit trail

### Key Features
- Soft deletes (`IsDeleted`, `DeletedAt`)
- Audit timestamps (`CreatedAt`, `ModifiedAt`)
- Foreign key constraints
- Optimized indexes

## ?? Testing

Default balance for new users: **10,000**

Use Swagger UI or Postman to test endpoints.

## ?? Future Enhancements

- [ ] Email Service Integration (SendGrid/AWS SES)
- [ ] 2FA Authentication
- [ ] Rate Limiting
- [ ] Redis Caching
- [ ] Game Controllers (Blackjack, Roulette, Slots)
- [ ] Wallet/Transaction System
- [ ] Admin Dashboard Endpoints
- [ ] Withdrawal/Deposit Endpoints with KYC verification
- [ ] WebSocket Support (real-time games)
- [ ] Background Jobs (token cleanup, expired KYC)

## ?? License

[Add your license here]

## ?? Contributing

[Add contributing guidelines here]

## ?? Support

[Add support contact information here]
