# OAuth2 Token Endpoint Documentation

## Overview

The Casino API now supports OAuth2-style authentication via the `/api/auth/token` endpoint. This endpoint uses the `password` grant type and requires a valid WebAPI key for authentication.

## Endpoint Details

**URL:** `POST /api/auth/token`

**Content-Type:** 
- `application/x-www-form-urlencoded` (recommended for OAuth2)
- `application/json`

**Authentication:** Requires valid `webapi_key`

**Token Expiration:** 30 minutes

---

## Request Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `grant_type` | string | Yes | Must be `"password"` |
| `username` | string | Yes | User's email or username |
| `password` | string | Yes | User's password |
| `webapi_key` | string | Yes | Valid API key for tenant validation |

---

## Request Examples

### Using Form Data (Recommended)

```bash
curl -X POST https://localhost:7001/api/auth/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "username=player1@example.com" \
  -d "password=Password123!" \
  -d "webapi_key=default_tenant_api_key_12345"
```

### Using JSON

```bash
curl -X POST https://localhost:7001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "grant_type": "password",
    "username": "player1@example.com",
    "password": "Password123!",
    "webapi_key": "default_tenant_api_key_12345"
  }'
```

### PowerShell Example

```powershell
$body = @{
    grant_type = "password"
  username = "player1@example.com"
    password = "Password123!"
 webapi_key = "default_tenant_api_key_12345"
}

Invoke-RestMethod -Uri "https://localhost:7001/api/auth/token" `
    -Method Post `
    -ContentType "application/json" `
    -Body ($body | ConvertTo-Json)
```

### JavaScript (Fetch API)

```javascript
const response = await fetch('https://localhost:7001/api/auth/token', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    grant_type: 'password',
    username: 'player1@example.com',
    password: 'Password123!',
    webapi_key: 'default_tenant_api_key_12345'
  })
});

const data = await response.json();
console.log('Access Token:', data.access_token);
```

---

## Success Response

**Status Code:** `200 OK`

**Response Body:**

```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 1800,
  "issued_at": "2024-01-15T10:30:00Z",
  "expires_at": "2024-01-15T11:00:00Z",
  "user": {
    "id": 1,
    "username": "player1",
    "email": "player1@example.com",
"balance": 1000.00,
    "created_at": "2024-01-10T08:00:00Z"
  }
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `access_token` | string | JWT bearer token for API authentication |
| `token_type` | string | Always `"Bearer"` |
| `expires_in` | integer | Token lifetime in seconds (1800 = 30 minutes) |
| `issued_at` | datetime | UTC timestamp when token was issued |
| `expires_at` | datetime | UTC timestamp when token expires |
| `user` | object | User information |

---

## Error Responses

### Invalid Grant Type

**Status Code:** `400 Bad Request`

```json
{
  "error": "unsupported_grant_type",
  "error_description": "Only 'password' grant type is supported"
}
```

### Invalid WebAPI Key

**Status Code:** `401 Unauthorized`

```json
{
  "error": "invalid_client",
  "error_description": "Invalid webapi_key"
}
```

### Invalid Credentials

**Status Code:** `401 Unauthorized`

```json
{
  "error": "invalid_grant",
  "error_description": "Invalid username or password"
}
```

### Missing Parameters

**Status Code:** `400 Bad Request`

```json
{
  "error": "invalid_request",
  "error_description": "Missing or invalid parameters"
}
```

---

## Using the Access Token

Once you receive the `access_token`, include it in the `Authorization` header for all API requests:

```bash
curl -X GET https://localhost:7001/api/wallet/balance \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Token Format

The token follows the JWT (JSON Web Token) standard and contains:

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload:**
```json
{
  "nameid": "1",
  "unique_name": "player1",
  "jti": "550e8400-e29b-41d4-a716-446655440000",
  "iat": 1705315800,
  "nbf": 1705315800,
  "exp": 1705317600,
  "iss": "CasinoBackend",
  "aud": "CasinoClients"
}
```

**Claims:**
- `nameid`: User ID
- `unique_name`: Username
- `jti`: Unique token ID
- `iat`: Issued at timestamp
- `exp`: Expiration timestamp (30 minutes from issuance)
- `iss`: Issuer (CasinoBackend)
- `aud`: Audience (CasinoClients)

---

## Default Credentials

### Test User
- **Username/Email:** `player1@example.com`
- **Password:** `Password123!`

### Admin User
- **Username:** `admin`
- **Email:** `admin@casinoapi.com`
- **Password:** `Admin@123`

### WebAPI Key
- **Key:** `default_tenant_api_key_12345`

---

## Token Expiration & Refresh

### Token Lifetime
- **Duration:** 30 minutes (1800 seconds)
- **Clock Skew:** None (strict expiration)

### Refresh Strategy
When the token expires, you must request a new token using the `/api/auth/token` endpoint with valid credentials.

**Recommended Practice:**
1. Store `expires_at` timestamp with the token
2. Request a new token 5 minutes before expiration
3. Implement automatic re-authentication on 401 responses

### Example Token Refresh Logic (JavaScript)

```javascript
class TokenManager {
  constructor() {
    this.token = null;
    this.expiresAt = null;
  }

  async getToken(credentials) {
    // Check if token exists and is still valid (with 5 min buffer)
    if (this.token && this.expiresAt) {
      const now = new Date();
      const expiryWithBuffer = new Date(this.expiresAt);
      expiryWithBuffer.setMinutes(expiryWithBuffer.getMinutes() - 5);

   if (now < expiryWithBuffer) {
    return this.token; // Token still valid
      }
    }

    // Request new token
    const response = await fetch('/api/auth/token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        grant_type: 'password',
        ...credentials
      })
    });

    const data = await response.json();
    this.token = data.access_token;
    this.expiresAt = new Date(data.expires_at);

    return this.token;
  }

  async makeAuthenticatedRequest(url, options = {}) {
    const token = await this.getToken();
    
    return fetch(url, {
  ...options,
      headers: {
        ...options.headers,
        'Authorization': `Bearer ${token}`
      }
    });
  }
}
```

---

## Security Considerations

### Best Practices

1. **HTTPS Only:** Always use HTTPS in production to encrypt credentials
2. **Secure Storage:** Store tokens securely:
   - Web: `sessionStorage` or encrypted `localStorage`
   - Mobile: Keychain (iOS) or Keystore (Android)
   - Desktop: Secure credential storage
3. **Token Scope:** Tokens have full user access - protect them like passwords
4. **Logging:** Never log tokens or credentials
5. **Rate Limiting:** Implement rate limiting on the token endpoint

### Configuration (appsettings.json)

```json
{
  "Jwt": {
    "Key": "your-super-secret-jwt-key-min-32-characters-long-for-security",
    "Issuer": "CasinoBackend",
    "Audience": "CasinoClients",
    "ExpireMinutes": 30
  }
}
```

**Important:** Change the `Jwt:Key` in production to a strong, randomly generated secret.

---

## Testing with Swagger

1. Navigate to `https://localhost:7001`
2. Find the `POST /api/auth/token` endpoint
3. Click "Try it out"
4. Fill in the request body:
   ```json
   {
     "grant_type": "password",
     "username": "player1@example.com",
   "password": "Password123!",
     "webapi_key": "default_tenant_api_key_12345"
   }
   ```
5. Click "Execute"
6. Copy the `access_token` from the response
7. Click "Authorize" at the top of the page
8. Enter: `Bearer <your_access_token>`
9. Now you can test other endpoints

---

## Comparison: Token Endpoint vs Login Endpoint

| Feature | `/api/auth/token` | `/api/auth/login` |
|---------|-------------------|-------------------|
| Standard | OAuth2 Password Grant | Custom |
| Content-Type | Form/JSON | JSON only |
| API Key | Required in body | Required in query string |
| Token Expiry | 30 minutes (configurable) | 120 minutes (default) |
| Response Format | OAuth2 standard | Custom |
| Use Case | OAuth2 clients, integrations | Legacy/simple apps |

**Recommendation:** Use `/api/auth/token` for new integrations as it follows OAuth2 standards.

---

## Troubleshooting

### Common Issues

**Problem:** `unsupported_grant_type` error

**Solution:** Ensure `grant_type` is exactly `"password"` (lowercase)

---

**Problem:** `invalid_client` error

**Solution:** 
- Verify the `webapi_key` is correct: `default_tenant_api_key_12345`
- Check that the API key exists and is active in the database

---

**Problem:** `invalid_grant` error

**Solution:**
- Verify username and password are correct
- Username can be email or username
- Ensure user account exists (register first if needed)

---

**Problem:** Token expires too quickly

**Solution:** 
- Token lifetime is 30 minutes by default
- Implement token refresh logic before expiration
- For development, you can increase expiry in `appsettings.Development.json`

---

**Problem:** 401 Unauthorized on API calls

**Solution:**
- Ensure token is included in `Authorization` header
- Format: `Authorization: Bearer <token>`
- Check token hasn't expired
- Verify token is valid JWT format

---

## Additional Endpoints

### Legacy Login Endpoint (Still Available)

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "player1@example.com",
  "password": "Password123!"
}
```

### Registration

```bash
POST /api/auth/register?apiKey=default_tenant_api_key_12345
Content-Type: application/json

{
  "username": "newplayer",
  "email": "newplayer@example.com",
  "password": "Password123!"
}
```

### Get Current User

```bash
GET /api/auth/me
Authorization: Bearer <token>
```

---

## Integration Examples

### React Application

```javascript
// authService.js
export const authenticate = async (username, password) => {
  const response = await fetch('/api/auth/token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      grant_type: 'password',
      username,
      password,
      webapi_key: process.env.REACT_APP_API_KEY
    })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error_description);
  }

  const data = await response.json();
  
  // Store token
  sessionStorage.setItem('access_token', data.access_token);
  sessionStorage.setItem('expires_at', data.expires_at);
  
  return data;
};

export const getAuthHeader = () => {
  const token = sessionStorage.getItem('access_token');
  return token ? `Bearer ${token}` : null;
};
```

### C# Client Application

```csharp
using System.Net.Http.Json;

public class CasinoApiClient
{
    private readonly HttpClient _http;
    private string? _accessToken;
    private DateTime? _expiresAt;

    public CasinoApiClient(HttpClient httpClient)
    {
_http = httpClient;
     _http.BaseAddress = new Uri("https://localhost:7001");
    }

    public async Task<TokenResponse> AuthenticateAsync(string username, string password)
    {
        var request = new
    {
      grant_type = "password",
   username,
            password,
     webapi_key = "default_tenant_api_key_12345"
        };

    var response = await _http.PostAsJsonAsync("/api/auth/token", request);
     response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
     
        _accessToken = tokenResponse.AccessToken;
        _expiresAt = tokenResponse.ExpiresAt;

    return tokenResponse;
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        _http.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);

        return await _http.GetFromJsonAsync<T>(endpoint);
    }
}
```

### Python Client

```python
import requests
from datetime import datetime, timedelta

class CasinoApiClient:
    def __init__(self, base_url="https://localhost:7001"):
        self.base_url = base_url
        self.access_token = None
        self.expires_at = None
    
    def authenticate(self, username, password, webapi_key):
        response = requests.post(
            f"{self.base_url}/api/auth/token",
          json={
  "grant_type": "password",
    "username": username,
              "password": password,
  "webapi_key": webapi_key
        }
        )
        response.raise_for_status()
        
        data = response.json()
        self.access_token = data["access_token"]
  self.expires_at = datetime.fromisoformat(data["expires_at"].replace('Z', '+00:00'))
        
        return data
    
    def get_headers(self):
        if not self.access_token:
raise Exception("Not authenticated")
        
        return {
     "Authorization": f"Bearer {self.access_token}"
        }
    
    def get(self, endpoint):
        response = requests.get(
       f"{self.base_url}{endpoint}",
            headers=self.get_headers()
        )
        response.raise_for_status()
      return response.json()
```

---

## Summary

The OAuth2 token endpoint provides a standardized way to authenticate with the Casino API:

? **Standard OAuth2** password grant flow  
? **30-minute token expiration** for security  
? **WebAPI key validation** for multi-tenancy  
? **Bearer token** authentication  
? **Supports username or email** for flexibility  
? **Detailed error responses** following OAuth2 spec  
? **Compatible with standard OAuth2 clients**

For any questions or issues, refer to the main API documentation or contact support.
