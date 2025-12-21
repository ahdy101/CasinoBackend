# ?? API Migration Guide - Old vs New

## What Changed

### Authentication Method
**Before:** JSON body with complex structure
```json
POST /api/auth/token
Content-Type: application/json

{
  "grant_type": "password",
  "username": "admin",
  "password": "admin123",
  "webapi_key": "default_tenant_api_key_12345"
}
```

**After:** Simple query parameters
```
POST /api/auth/token?grant_type=password&username=admin&password=admin123&webapi_key=default_tenant_api_key_12345
```

---

### Wallet Endpoints
**Before:** JSON body + Authorize button
```json
POST /api/Wallet/add-funds
Authorization: Bearer {token} (set via Authorize button)
Content-Type: application/json

{
  "amount": 10000
}
```

**After:** Query parameters + Authorization header
```
POST /api/Wallet/add-funds?apiKey=default_tenant_api_key_12345&amount=10000
Authorization: Bearer {token} (direct header parameter)
```

---

### Blackjack Endpoints
**Before:** JSON body
```json
POST /api/Blackjack/deal
Authorization: Bearer {token}
Content-Type: application/json

{
  "betAmount": 100
}
```

**After:** Query parameters
```
POST /api/Blackjack/deal?apiKey=default_tenant_api_key_12345&betAmount=100
Authorization: Bearer {token}
```

---

## Key Differences

| Aspect | Old API | New API |
|--------|---------|---------|
| Authorization | Authorize button in Swagger | Direct Authorization header parameter |
| Request Format | JSON body | Query parameters |
| API Key Location | Query or attribute | Always query parameter |
| Bearer Token | Hidden by Swagger auth | Visible in Authorization header |
| Swagger UI | Complex with auth modal | Simple text boxes only |

---

## Benefits of New Design

1. **? Simpler Testing:** Just fill text boxes in Swagger
2. **? No JSON Errors:** No syntax mistakes
3. **? Clear Parameters:** See all required values
4. **? Standard Practice:** Authorization header is HTTP standard
5. **? Easier for Beginners:** Like filling a form
6. **? No Hidden Auth:** Everything is visible

---

## Migration Steps

### Step 1: Understand New Parameter Locations

**Headers:**
- `Authorization: Bearer {token}` ? For authenticated endpoints

**Query Parameters:**
- `apiKey` ? For multi-tenant validation
- `amount`, `betAmount`, `gameId` ? Business logic parameters
- `grant_type`, `username`, `password`, `webapi_key` ? Auth parameters

### Step 2: Update Your Client Code

**JavaScript Example:**

Old:
```javascript
fetch('/api/Wallet/add-funds', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ amount: 10000 })
})
```

New:
```javascript
fetch('/api/Wallet/add-funds?apiKey=default_tenant_api_key_12345&amount=10000', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
```

**C# Example:**

Old:
```csharp
var content = new StringContent(
    JsonSerializer.Serialize(new { amount = 10000 }),
  Encoding.UTF8,
    "application/json"
);
await httpClient.PostAsync("/api/Wallet/add-funds", content);
```

New:
```csharp
await httpClient.PostAsync(
    "/api/Wallet/add-funds?apiKey=default_tenant_api_key_12345&amount=10000",
    null
);
```

### Step 3: Remove Swagger Authorization Setup

Old (no longer needed):
```javascript
// Don't need this anymore
const swaggerAuthorize = () => {
  document.querySelector('.authorize-button').click();
  // ... complex auth modal handling
};
```

New (not needed):
```javascript
// Authorization is just a regular parameter now
// No special handling required!
```

---

## Testing the Migration

### Test Checklist:

1. **? Get Token**
   ```
   POST /api/auth/token?grant_type=password&username=admin&password=admin123&webapi_key=default_tenant_api_key_12345
   ```

2. **? Test Wallet**
   ```
   POST /api/Wallet/add-funds?apiKey=default_tenant_api_key_12345&amount=10000
   Headers: Authorization: Bearer {token}
   ```

3. **? Test Blackjack**
   ```
   POST /api/Blackjack/deal?apiKey=default_tenant_api_key_12345&betAmount=100
   Headers: Authorization: Bearer {token}
   ```

4. **? Test Admin (with admin token)**
   ```
   GET /api/Admin/dashboard
   Headers: Authorization: Bearer {admin_token}
   ```

5. **? Test Regular User Cannot Access Admin**
   ```
   GET /api/Admin/dashboard
   Headers: Authorization: Bearer {user_token}
   Expected: 403 Forbidden
   ```

---

## Common Migration Issues

### Issue 1: "Authorization header is missing"
**Cause:** Forgot to add Authorization header  
**Fix:** Add `Authorization: Bearer {token}` to headers

### Issue 2: "Invalid API key"
**Cause:** API key parameter missing or wrong  
**Fix:** Add `?apiKey=default_tenant_api_key_12345` to URL

### Issue 3: "Missing required parameters"
**Cause:** Parameters in body instead of query string  
**Fix:** Move parameters from JSON body to query string

### Issue 4: Token validation fails
**Cause:** Token format wrong or expired  
**Fix:** Ensure format is `Bearer {token}` with space, get new token if expired

---

## Backwards Compatibility

**Breaking Changes:**
- ? JSON body requests no longer accepted
- ? Swagger Authorize button removed
- ? `[FromBody]` endpoints changed to `[FromQuery]`

**Migration Required:**
- All client applications must update
- Update API documentation
- Update integration tests
- Update Postman collections

---

## Rollback Plan (If Needed)

If you need to rollback to old behavior:

1. Restore old controller files from git
2. Re-add Swagger security configuration
3. Rebuild and redeploy

**Git Command:**
```bash
git checkout HEAD~1 Controllers/
git checkout HEAD~1 Program.cs
```

---

## Support

For questions or issues during migration:
1. Check `SIMPLIFIED_API_GUIDE.md` for complete endpoint reference
2. Test in Swagger UI first before updating client code
3. Verify token format: `Bearer {token}` with space

---

**Migration is straightforward - just move parameters from JSON body to query string!** ??
