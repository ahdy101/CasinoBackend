# ? SIMPLIFIED API - TEXT BOX PARAMETERS ONLY

## ?? What Changed

### Before (Complex):
- ? JSON body requests
- ? Multiple authorization methods
- ? Confusing "Authorize" button in Swagger
- ? Complex request bodies

### After (Simple):
- ? Simple text box parameters for everything
- ? Bearer token as a regular parameter (Authorization header)
- ? API key as query parameter  
- ? All values as query parameters (like a form)
- ? NO "Authorize" button - cleaner Swagger UI

---

## ?? Quick Start Guide

### Step 1: Get Your Bearer Token

**Endpoint:** `POST /api/auth/token`

**Parameters (all in text boxes):**
- grant_type: `password`
- username: `admin`
- password: `admin123`
- webapi_key: `default_tenant_api_key_12345`

**Response:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 1800
}
```

**Copy the `access_token` value** - you'll need it for all other endpoints!

---

## ?? Using Endpoints in Swagger

### Example: Add Funds to Wallet

1. Find `POST /api/Wallet/add-funds`
2. Click "Try it out"
3. Fill in the text boxes:
   - **Authorization** (header): `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
   - **apiKey**: `default_tenant_api_key_12345`
   - **amount**: `10000`
4. Click "Execute"

**That's it!** No JSON, no complex forms!

---

## ?? All Endpoints Reference

### ?? Authentication (No Bearer Token Required)

#### 1. Get Bearer Token
```
POST /api/auth/token
Parameters:
  - grant_type: "password"
  - username: "admin"
  - password: "admin123"
  - webapi_key: "default_tenant_api_key_12345"
```

#### 2. Register New User
```
POST /api/auth/register
Parameters:
  - username: "newuser"
  - email: "user@example.com"
  - password: "Password123"
  - apiKey: "default_tenant_api_key_12345"
```

#### 3. Get Current User Info
```
GET /api/auth/me
Headers:
- Authorization: "Bearer {your_token}"
```

---

### ?? Wallet Endpoints (Requires Bearer Token + API Key)

#### 1. Get Balance
```
GET /api/Wallet/balance
Headers:
  - Authorization: "Bearer {your_token}"
Parameters:
  - apiKey: "default_tenant_api_key_12345"
```

#### 2. Add Funds
```
POST /api/Wallet/add-funds
Headers:
  - Authorization: "Bearer {your_token}"
Parameters:
  - apiKey: "default_tenant_api_key_12345"
  - amount: 10000
```

#### 3. Cash Out
```
POST /api/Wallet/cash-out
Headers:
  - Authorization: "Bearer {your_token}"
Parameters:
  - apiKey: "default_tenant_api_key_12345"
  - amount: 5000
```

---

### ?? Blackjack Endpoints (Requires Bearer Token + API Key)

#### 1. Deal New Game
```
POST /api/Blackjack/deal
Headers:
  - Authorization: "Bearer {your_token}"
Parameters:
  - apiKey: "default_tenant_api_key_12345"
  - betAmount: 100
```

#### 2. Hit (Draw Card)
```
POST /api/Blackjack/hit
Headers:
  - Authorization: "Bearer {your_token}"
Parameters:
  - apiKey: "default_tenant_api_key_12345"
  - gameId: 1
```

#### 3. Stand (End Turn)
```
POST /api/Blackjack/stand
Headers:
  - Authorization: "Bearer {your_token}"
Parameters:
  - apiKey: "default_tenant_api_key_12345"
  - gameId: 1
```

#### 4. Double Down
```
POST /api/Blackjack/double-down
Headers:
  - Authorization: "Bearer {your_token}"
Parameters:
  - apiKey: "default_tenant_api_key_12345"
  - gameId: 1
```

---

### ?? Admin Endpoints (Requires Admin Bearer Token)

#### 1. Get Dashboard
```
GET /api/Admin/dashboard
Headers:
  - Authorization: "Bearer {admin_token}"
```

#### 2. Get User Statistics
```
GET /api/Admin/users
Headers:
  - Authorization: "Bearer {admin_token}"
```

#### 3. Get Transaction Statistics
```
GET /api/Admin/transactions
Headers:
  - Authorization: "Bearer {admin_token}"
```

#### 4. Get Game Statistics
```
GET /api/Admin/games
Headers:
  - Authorization: "Bearer {admin_token}"
```

---

## ?? CURL Examples

### Get Token
```bash
curl -X POST "http://localhost:5001/api/auth/token?grant_type=password&username=admin&password=admin123&webapi_key=default_tenant_api_key_12345"
```

### Add Funds
```bash
curl -X POST "http://localhost:5001/api/Wallet/add-funds?apiKey=default_tenant_api_key_12345&amount=10000" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Deal Blackjack
```bash
curl -X POST "http://localhost:5001/api/Blackjack/deal?apiKey=default_tenant_api_key_12345&betAmount=100" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Get Admin Dashboard
```bash
curl -X GET "http://localhost:5001/api/Admin/dashboard" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN_HERE"
```

---

## ?? Tips for Swagger UI

### Text Box Tips:
1. **Authorization Header:** Always starts with `Bearer ` (with space)
   - Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

2. **API Key:** Just the key value, no prefix
   - Example: `default_tenant_api_key_12345`

3. **Amounts:** Enter numbers without quotes
   - Example: `10000` (not `"10000"`)

4. **Game ID:** Just the number
   - Example: `1` (not `"1"`)

### Common Mistakes:
- ? Forgetting `Bearer ` prefix in Authorization header
- ? Using quotes around numbers
- ? Typing "authorization" in the text box (it's a header parameter, not query)
- ? Using expired tokens (tokens last 30 minutes)

---

## ?? Regular User vs Admin

### Regular User Can Access:
- ? All Wallet endpoints
- ? All Blackjack endpoints
- ? Auth/me endpoint
- ? Admin endpoints (will get 403 Forbidden)

### Admin User Can Access:
- ? All Wallet endpoints
- ? All Blackjack endpoints
- ? All Admin endpoints
- ? Auth/me endpoint

### How to Tell:
Check the `role` in your token response:
- `"role": "User"` ? Regular user
- `"role": "Admin"` ? Admin user

---

## ?? Response Codes

| Code | Meaning | Common Cause |
|------|---------|--------------|
| 200 | Success | Everything worked! |
| 400 | Bad Request | Missing parameters or invalid values |
| 401 | Unauthorized | Invalid or expired token |
| 403 | Forbidden | You don't have permission (wrong role) |
| 404 | Not Found | Resource doesn't exist |
| 500 | Server Error | Something went wrong on server |

---

## ?? Token Expiration

**Tokens expire in 30 minutes.**

### When Your Token Expires:
1. You'll get `401 Unauthorized` response
2. Error message: "Token has expired"
3. Solution: Get a new token using `/api/auth/token`

### Pro Tip:
Set a reminder or timer for 25 minutes after getting a token!

---

## ? Testing Checklist

### First Time Setup:
- [ ] Database is created (drop and recreate if needed)
- [ ] API is running (`dotnet run`)
- [ ] Can access Swagger UI at `http://localhost:5001`

### Get Started:
- [ ] Get bearer token from `/api/auth/token`
- [ ] Copy the `access_token` value
- [ ] Test wallet endpoint with your token
- [ ] Verify balance increased

### Test Admin Access:
- [ ] Login as admin user
- [ ] Get admin token
- [ ] Access `/api/Admin/dashboard`
- [ ] Should get 200 OK with dashboard data

### Test Regular User:
- [ ] Register a new user
- [ ] Get token for new user
- [ ] Try to access `/api/Admin/dashboard`
- [ ] Should get 403 Forbidden

---

## ?? Benefits of New Design

1. **Simpler for Beginners:** Just fill in text boxes
2. **No JSON Confusion:** No need to format JSON correctly
3. **Clear Parameters:** See exactly what's needed
4. **Easy Testing:** Quick to test endpoints in Swagger
5. **No Authorization Button:** Less confusion in UI
6. **Standard HTTP:** Bearer token in Authorization header (standard practice)

---

## ?? Quick Reference

**Default Admin:**
- Username: `admin`
- Password: `admin123`
- Email: `admin@casinoapi.com`

**Default API Key:**
- `default_tenant_api_key_12345`

**Token Format:**
- `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

**Token Expires In:**
- 30 minutes (1800 seconds)

---

**That's it! No complex JSON, no confusing authorization - just simple text boxes!** ??
