# Security Configuration Guide

## ?? IMPORTANT: Before Pushing to GitHub

### 1. Protect Sensitive Configuration Files

The following files contain sensitive information and are **excluded from Git**:
- `Casino_Admin/appsettings.json`
- `Casino_Api/appsettings.json`
- Any `appsettings.*.json` files

### 2. Setup Instructions for New Developers

#### Database Configuration

1. Copy `Casino_Admin/appsettings.EXAMPLE.json` to `Casino_Admin/appsettings.json`
2. Copy `Casino_Api/appsettings.EXAMPLE.json` to `Casino_Api/appsettings.json`
3. Update the connection strings with your MySQL credentials:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=casino_db;User=YOUR_USERNAME;Password=YOUR_PASSWORD;"
}
```

#### JWT Configuration (API Only)

Update the JWT key in `Casino_Api/appsettings.json`:

```json
"Jwt": {
  "Key": "GENERATE_A_STRONG_RANDOM_KEY_AT_LEAST_32_CHARACTERS",
"Issuer": "CasinoBackend",
  "Audience": "CasinoUsers",
  "ExpireMinutes": 120
}
```

**Generate a secure JWT key** using PowerShell:
```powershell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

Or using an online generator (use HTTPS): https://randomkeygen.com/

### 3. Database Security

#### Create MySQL User with Limited Privileges

```sql
-- Create database
CREATE DATABASE casino_db;

-- Create user with password
CREATE USER 'casino_user'@'localhost' IDENTIFIED BY 'STRONG_PASSWORD_HERE';

-- Grant only necessary privileges
GRANT SELECT, INSERT, UPDATE, DELETE ON casino_db.* TO 'casino_user'@'localhost';
FLUSH PRIVILEGES;
```

### 4. API Key Security

#### Generate Secure API Keys

Use a cryptographically secure random string for API keys:

**PowerShell:**
```powershell
[System.Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

**C#:**
```csharp
var apiKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
```

#### Insert API Key into Database

```sql
INSERT INTO TenantApiKeys (TenantName, ApiKey, CreatedAt)
VALUES ('your-tenant-name', 'YOUR_GENERATED_API_KEY_HERE', NOW());
```

### 5. Production Deployment Checklist

- [ ] Use environment variables for sensitive data
- [ ] Enable HTTPS only (disable HTTP in production)
- [ ] Set strong, unique passwords for database users
- [ ] Rotate JWT keys regularly
- [ ] Rotate API keys regularly
- [ ] Use a proper secrets manager (Azure Key Vault, AWS Secrets Manager, etc.)
- [ ] Enable rate limiting on API endpoints
- [ ] Add CORS policies (restrict origins)
- [ ] Enable SQL injection protection (parameterized queries - already done)
- [ ] Review and restrict file upload sizes
- [ ] Enable audit logging for admin actions
- [ ] Set secure cookie policies (HttpOnly, Secure, SameSite)

### 6. Environment Variables (Recommended for Production)

Instead of `appsettings.json`, use environment variables:

#### Windows (PowerShell):
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=...;Password=..."
$env:Jwt__Key = "your-jwt-key"
```

#### Linux/Mac:
```bash
export ConnectionStrings__DefaultConnection="Server=...;Password=..."
export Jwt__Key="your-jwt-key"
```

#### Azure App Service:
Add as Application Settings in the Azure Portal.

### 7. Never Commit These Files

- `appsettings.json` (already in .gitignore)
- `appsettings.Development.json`
- `appsettings.Production.json`
- `.env` files
- Private keys or certificates

### 8. What to Do If You Accidentally Committed Secrets

1. **Immediately rotate all exposed credentials**
2. Change database passwords
3. Generate new JWT keys
4. Generate new API keys
5. Use `git filter-branch` or BFG Repo-Cleaner to remove from history
6. Force push to remote (if you own the repository)

### 9. Additional Security Measures

#### Enable HTTPS Redirection
Already configured in `Program.cs`:
```csharp
app.UseHttpsRedirection();
```

#### Add Rate Limiting (Recommended)

Install package:
```bash
dotnet add package AspNetCoreRateLimit
```

#### Add Security Headers

Consider adding:
- Content Security Policy (CSP)
- X-Content-Type-Options
- X-Frame-Options
- Strict-Transport-Security (HSTS)

### 10. Contact

For security issues, please contact the repository owner privately.

## License

This security guide is part of the Casino Backend project.
