# Authentication Endpoints with Tenant API Key - Complete Guide

## Overview
The authentication endpoints (Register and Login) now require a valid tenant API key for security and multi-tenancy support.

## Changes Made

### 1. AuthController Updated
**File:** `Casino_Api/Controllers/AuthController.cs`

**Added:**
- `ITenantApiKeyRepository` dependency injection
- `IsApiKeyValid()` private method for validation
- `apiKey` query parameter to both Register and Login endpoints
- Proper error responses for invalid/missing API keys

### 2. TenantApiKey Model Updated
**File:** `Casino_Api/Models/TenantApiKey.cs`

**Added:**
- `IsActive` property (bool, default true) - Allows enabling/disabling API keys

### 3. TenantApiKeyRepository Updated
**File:** `Casino_Api/Repositories/Implementations/TenantApiKeyRepository.cs`

**Updated:**
- All SQL queries now include `IsActive` column
- `ValidateApiKeyAsync()` now checks both ApiKey AND IsActive = 1

### 4. Database Migration Created
**File:** `Casino_Api/Migrations/SQL/20250104_AddIsActiveToTenantApiKeys.sql`

**Adds:**
- `IsActive` column to TenantApiKeys table
- Indexes for performance optimization

---

## Migration Steps

### Step 1: Apply TenantApiKeys Migration

Run in MySQL:

```sql
USE casino_db;

-- Add IsActive column
ALTER TABLE TenantApiKeys 
ADD COLUMN IsActive BOOLEAN NOT NULL DEFAULT 1;

-- Add indexes
CREATE INDEX IX_TenantApiKeys_IsActive ON TenantApiKeys(IsActive);
CREATE INDEX IX_TenantApiKeys_ApiKey_IsActive ON TenantApiKeys(ApiKey, IsActive);
```

Or use the migration file:
```bash
mysql -u casino_user -p casino_db < Casino_Api/Migrations/SQL/20250104_AddIsActiveToTenantApiKeys.sql
```

### Step 2: Verify/Create Default API Key

Check if you have an API key in the database:

```sql
SELECT * FROM TenantApiKeys WHERE IsActive = 1;
```

If no keys exist, create one:

```sql
INSERT INTO TenantApiKeys (TenantName, ApiKey, IsActive, CreatedAt)
VALUES ('DefaultTenant', 'default_tenant_api_key_12345', 1, NOW());
```

### Step 3: Restart Your Application

1. Stop debugging (Shift+F5)
2. Rebuild (Ctrl+Shift+B)
3. Start debugging (F5)

---

## API Usage

### 1. Register a New User

**Endpoint:**
```
POST https://localhost:<port>/api/auth/register?apiKey=default_tenant_api_key_12345
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "player1",
  "password": "SecurePass123!",
  "initialBalance": 1000
}
```

**Success Response (201 Created):**
```json
{
  "id": 1,
  "username": "player1",
  "balance": 1000.00,
  "createdAt": "2025-01-04T12:00:00Z"
}
```

**Error Response (401 Unauthorized) - Missing/Invalid API Key:**
```json
{
  "message": "Invalid or missing API key",
  "errors": [
    "A valid API key is required for registration"
  ]
}
```

**Error Response (409 Conflict) - Username Already Exists:**
```json
{
  "message": "Username taken",
  "errors": [
 "Username taken"
  ]
}
```

---

### 2. Login and Get JWT Token

**Endpoint:**
```
POST https://localhost:<port>/api/auth/login?apiKey=default_tenant_api_key_12345
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "player1",
  "password": "SecurePass123!"
}
```

**Success Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InBsYXllcjEiLCJuYW1laWQiOiIxIiwibmJmIjoxNzA0MzY3MjAwLCJleHAiOjE3MDQzNzQ0MDAsImlhdCI6MTcwNDM2NzIwMCwiaXNzIjoiQ2FzaW5vQmFja2VuZCIsImF1ZCI6IkNhc2lub1VzZXJzIn0.xxxxx",
  "username": "player1"
}
```

**Error Response (401 Unauthorized) - Invalid Credentials:**
```json
{
  "message": "Invalid credentials",
  "errors": [
    "Username or password is incorrect"
  ]
}
```

**Error Response (401 Unauthorized) - Missing/Invalid API Key:**
```json
{
  "message": "Invalid or missing API key",
  "errors": [
    "A valid API key is required for login"
  ]
}
```

---

## Testing with Swagger UI

### 1. Open Swagger
Navigate to: `https://localhost:<port>/swagger`

### 2. Test Register

1. Find `POST /api/auth/register`
2. Click "Try it out"
3. **Important:** Add `apiKey` parameter in the query parameters section
4. Fill in the request body:
```json
{
  "username": "testuser",
  "password": "Test123!",
  "initialBalance": 500
}
```
5. Click "Execute"
6. Should return 201 Created with user data

### 3. Test Login

1. Find `POST /api/auth/login`
2. Click "Try it out"
3. **Important:** Add `apiKey` parameter: `default_tenant_api_key_12345`
4. Fill in the request body:
```json
{
  "username": "testuser",
  "password": "Test123!"
}
```
5. Click "Execute"
6. Copy the JWT token from the response

### 4. Use JWT Token for Protected Endpoints

1. Click the "Authorize" button at the top of Swagger UI
2. Enter: `Bearer <your-jwt-token>`
3. Click "Authorize"
4. Now you can call protected endpoints like game endpoints

---

## Testing with PowerShell

### Register User
```powershell
$registerBody = @{
    username = "player2"
    password = "MyPassword123!"
    initialBalance = 2000
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "https://localhost:7001/api/auth/register?apiKey=default_tenant_api_key_12345" `
    -Method Post `
    -ContentType "application/json" `
    -Body $registerBody `
  -SkipCertificateCheck
```

### Login
```powershell
$loginBody = @{
 username = "player2"
    password = "MyPassword123!"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:7001/api/auth/login?apiKey=default_tenant_api_key_12345" `
 -Method Post `
 -ContentType "application/json" `
    -Body $loginBody `
    -SkipCertificateCheck

$token = $response.token
Write-Host "JWT Token: $token"
```

### Use Token in Protected Endpoint
```powershell
Invoke-RestMethod `
    -Uri "https://localhost:7001/api/users?apiKey=default_tenant_api_key_12345" `
    -Method Get `
    -Headers @{ Authorization = "Bearer $token" } `
    -SkipCertificateCheck
```

---

## Common Issues and Solutions

### Issue 1: "Invalid or missing API key"

**Cause:** API key not provided or invalid

**Solutions:**
1. Make sure to add `?apiKey=default_tenant_api_key_12345` to the URL
2. Check that the API key exists in the database:
```sql
SELECT * FROM TenantApiKeys WHERE IsActive = 1;
```
3. If no key exists, create one (see Step 2 above)

---

### Issue 2: "Username or password is incorrect"

**Cause:** Invalid credentials or user doesn't exist

**Solutions:**
1. Register the user first if not exists
2. Check username and password are correct
3. Verify user exists in database:
```sql
SELECT Id, Username, CreatedAt FROM Users WHERE Username = 'your_username';
```

---

### Issue 3: Token Expires

**Cause:** JWT tokens expire after configured time (default: 120 minutes)

**Solution:**
1. Login again to get a new token
2. Adjust `Jwt:ExpireMinutes` in `appsettings.json` if needed

---

### Issue 4: Cannot access protected endpoints even with token

**Cause:** Token not properly formatted in Authorization header

**Solutions:**
1. Make sure Authorization header is: `Bearer <token>` (with space after Bearer)
2. Verify token is valid (not expired)
3. Check Swagger "Authorize" button shows a valid token

---

## Database Verification Queries

### Check All API Keys
```sql
SELECT Id, TenantName, ApiKey, IsActive, CreatedAt 
FROM TenantApiKeys
ORDER BY CreatedAt DESC;
```

### Check All Users
```sql
SELECT Id, Username, Balance, CreatedAt 
FROM Users
ORDER BY CreatedAt DESC;
```

### Activate/Deactivate an API Key
```sql
-- Deactivate
UPDATE TenantApiKeys SET IsActive = 0 WHERE Id = 1;

-- Activate
UPDATE TenantApiKeys SET IsActive = 1 WHERE Id = 1;
```

---

## Security Best Practices

1. **Production API Keys:**
   - Generate cryptographically secure random API keys
   - Use at least 32 characters
   - Rotate keys periodically

2. **JWT Tokens:**
   - Store tokens securely (never in localStorage for sensitive apps)
   - Use HTTPS only in production
   - Set appropriate expiration times

3. **Passwords:**
   - Enforce strong password policies
   - Passwords are automatically hashed with BCrypt
   - Never log or expose passwords

4. **API Key Management:**
   - Use different API keys per tenant/environment
   - Monitor API key usage
- Disable compromised keys immediately (set IsActive = 0)

---

## Complete Flow Example

```
1. Client: Register User
   POST /api/auth/register?apiKey=xxx
   Body: { username, password, initialBalance }
   
   ?
   
2. Server: Validate API Key ? Create User ? Return User Data

3. Client: Login
   POST /api/auth/login?apiKey=xxx
   Body: { username, password }
   
   ?
   
4. Server: Validate API Key ? Verify Credentials ? Generate JWT ? Return Token

5. Client: Use JWT for Protected Endpoints
   GET /api/users?apiKey=xxx
   Header: Authorization: Bearer <jwt-token>
   
   ?
   
6. Server: Validate API Key ? Validate JWT ? Process Request
```

---

## Testing Checklist

- [ ] Apply TenantApiKeys migration (add IsActive column)
- [ ] Verify API key exists in database and is active
- [ ] Restart application
- [ ] Test Register without API key (should fail with 401)
- [ ] Test Register with invalid API key (should fail with 401)
- [ ] Test Register with valid API key (should succeed with 201)
- [ ] Test Login without API key (should fail with 401)
- [ ] Test Login with invalid API key (should fail with 401)
- [ ] Test Login with valid credentials and API key (should succeed with 200)
- [ ] Test Login with invalid credentials (should fail with 401)
- [ ] Verify JWT token works for protected endpoints
- [ ] Test token expiration after configured time

---

**Implementation Date:** January 4, 2025  
**Version:** 2.0  
**Security Level:** Production-Ready with Multi-Tenant Support
