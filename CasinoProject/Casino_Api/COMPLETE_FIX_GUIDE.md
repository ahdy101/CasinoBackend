# Complete Fix Guide: API Errors + Role-Based Authorization

## ?? Your Errors Explained

### Error 1: Double "Bearer" in Authorization Header
```bash
# ? WRONG:
-H 'Authorization: Bearer Bearer eyJ...'

# ? CORRECT:
-H 'Authorization: Bearer eyJ...'
```

**Why:** Swagger adds "Bearer" automatically. You only paste the token.

### Error 2: Missing API Key
```bash
# ? WRONG:
curl -X 'POST' 'http://localhost:5001/api/Wallet/add-funds'

# ? CORRECT:
curl -X 'POST' 'http://localhost:5001/api/Wallet/add-funds?apiKey=default_tenant_api_key_12345'
```

**Why:** WalletController has `[ServiceFilter(typeof(RequireApiKeyAttribute))]`

---

## ? Complete Solution Implemented

### Changes Made:

1. **? Added Role Property to User Model**
   - New column: `Role` (varchar(20), default: "User")
   - Possible values: "User", "Admin", "SuperAdmin"

2. **? Updated JWT Token Generation**
   - Now includes `ClaimTypes.Role` claim
   - Admin token includes `role: "Admin"`
   - Regular user token includes `role: "User"`

3. **? Added Role-Based Authorization**
   - AdminController: `[Authorize(Roles = "Admin,SuperAdmin")]`
   - WalletController: Regular `[Authorize]` (all authenticated users)
   - BlackjackController: Regular `[Authorize]` (all authenticated users)

4. **? Updated Database Seed**
 - Admin user now has `Role = "Admin"`

---

## ?? Required Database Update

### Step 1: Drop and Recreate Database

**Run this SQL in MySQL Workbench:**
```sql
DROP DATABASE IF EXISTS casino_db;
CREATE DATABASE casino_db;
```

### Step 2: Restart the API
```powershell
cd C:\Users\ahmad\source\repos\CasinoBackend\CasinoProject\Casino_Api
dotnet clean
dotnet build
dotnet run
```

The API will automatically create all tables with the new `Role` column!

---

## ?? Testing the Fix

### Test 1: Get Admin Token

**Request:**
```bash
POST http://localhost:5001/api/auth/token
Content-Type: application/json

{
  "grant_type": "password",
  "username": "admin",
  "password": "admin123",
  "webapi_key": "default_tenant_api_key_12345"
}
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 1800,
  "user": {
    "id": 999,
    "username": "admin",
    "email": "admin@casinoapi.com",
    "balance": 999999.00,
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

**The token now contains:** `"role": "Admin"` (in JWT claims)

### Test 2: Call Wallet Endpoint (CORRECT WAY)

```bash
curl -X 'POST' \
  'http://localhost:5001/api/Wallet/add-funds?apiKey=default_tenant_api_key_12345' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijk5OSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhZG1pbiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiYTRiYjM1NmUtNzg5MC00ZWQ4LTk3ZTQtODc2NTQzMjEwOTg3IiwiaWF0IjoiMTczNzAyMTY1MCIsImV4cCI6MTczNzAyMzQ1MCwiaXNzIjoiQ2FzaW5vQmFja2VuZCIsImF1ZCI6IkNhc2lub0NsaWVudHMifQ.LxKExample_Token_Here' \
  -H 'Content-Type: application/json' \
  -d '{"amount": 100000}'
```

**Key Points:**
- ? Single "Bearer" keyword
- ? API key in query string: `?apiKey=...`
- ? Full token after Bearer (no truncation)

### Test 3: Admin Endpoint Access

**Admin User (Role="Admin"):**
```bash
GET http://localhost:5001/api/Admin/dashboard
Authorization: Bearer {admin_token}
```
**Result:** ? 200 OK (Access granted)

**Regular User (Role="User"):**
```bash
GET http://localhost:5001/api/Admin/dashboard
Authorization: Bearer {user_token}
```
**Result:** ? 403 Forbidden (Access denied)

---

## ?? Creating a Regular User

### Register New User:
```bash
POST http://localhost:5001/api/auth/register?apiKey=default_tenant_api_key_12345
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test@123"
}
```

This user will have `Role = "User"` by default.

### Get Token for Regular User:
```bash
POST http://localhost:5001/api/auth/token
Content-Type: application/json

{
  "grant_type": "password",
  "username": "test@example.com",
  "password": "Test@123",
  "webapi_key": "default_tenant_api_key_12345"
}
```

---

## ?? Access Control Summary

| Endpoint | Admin | Regular User |
|----------|-------|--------------|
| POST /api/auth/token | ? Yes | ? Yes |
| POST /api/auth/register | ? Yes | ? Yes |
| POST /api/auth/login | ? Yes | ? Yes |
| GET /api/auth/me | ? Yes | ? Yes |
| **POST /api/Wallet/add-funds** | ? Yes | ? Yes |
| **GET /api/Wallet/balance** | ? Yes | ? Yes |
| **POST /api/Blackjack/deal** | ? Yes | ? Yes |
| **GET /api/Admin/dashboard** | ? Yes | ? No (403) |
| **GET /api/Admin/users** | ? Yes | ? No (403) |
| **GET /api/Admin/transactions** | ? Yes | ? No (403) |

---

## ?? Swagger UI Testing

### Step 1: Get Token
1. Go to `http://localhost:5001`
2. Find **POST /api/auth/token**
3. Click "Try it out"
4. Paste:
```json
{
  "grant_type": "password",
  "username": "admin",
  "password": "admin123",
  "webapi_key": "default_tenant_api_key_12345"
}
```
5. Click "Execute"
6. **Copy only the access_token value** (no "Bearer" prefix)

### Step 2: Authorize
1. Click **"Authorize"** button (top right, lock icon)
2. Two fields will appear:
   - **Bearer (http, Bearer)**: Paste the token HERE (no "Bearer" word)
   - **ApiKey (apiKey, Query)**: Paste `default_tenant_api_key_12345`
3. Click "Authorize"
4. Click "Close"

### Step 3: Test Endpoints
Now all endpoints work! The authorization is automatic.

---

## ?? Common Errors & Solutions

### Error: "Unauthorized"
**Cause:** Token expired or invalid  
**Solution:** Get a new token (tokens expire in 30 minutes)

### Error: "Forbidden" (403)
**Cause:** User role doesn't have permission  
**Solution:** Use admin token for admin endpoints

### Error: "Invalid API key"
**Cause:** Missing or wrong API key in query string  
**Solution:** Add `?apiKey=default_tenant_api_key_12345` to URL

### Error: "The JSON value could not be converted"
**Cause:** Database schema out of sync  
**Solution:** Drop and recreate database (see above)

---

## ?? Quick Commands Summary

```sql
-- MySQL: Drop database
DROP DATABASE IF EXISTS casino_db;
CREATE DATABASE casino_db;
```

```powershell
# PowerShell: Rebuild and run
dotnet clean
dotnet build
dotnet run
```

```bash
# Get admin token
curl -X POST http://localhost:5001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"grant_type":"password","username":"admin","password":"admin123","webapi_key":"default_tenant_api_key_12345"}'
```

```bash
# Test wallet (replace YOUR_TOKEN_HERE)
curl -X POST "http://localhost:5001/api/Wallet/add-funds?apiKey=default_tenant_api_key_12345" \
-H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{"amount":10000}'
```

---

## ? Checklist

- [ ] Drop and recreate database
- [ ] Restart API (dotnet run)
- [ ] Get admin token
- [ ] Test with single "Bearer" keyword
- [ ] Include API key in query string
- [ ] Verify admin endpoints return 403 for regular users
- [ ] Create a regular user and test access

---

**Everything is now properly configured with role-based authorization!** ??
