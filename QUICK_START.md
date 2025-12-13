# Quick Setup Script

## Step 1: Create API Key (Run this SQL in your MySQL database)

```sql
USE casino_db;

-- Create a test API key
INSERT INTO TenantApiKeys (TenantName, ApiKey, CreatedAt)
VALUES ('TestTenant', 'test-api-key-123', UTC_TIMESTAMP());

-- Verify it was created
SELECT * FROM TenantApiKeys;
```

## Step 2: Run the API

Open a terminal and run:
```bash
cd C:\Users\ahmad\source\repos\CasinoBackend\Casino_Api
dotnet run
```

## Step 3: Open Swagger

The console will show the URL, something like:
```
Now listening on: https://localhost:7XXX
```

Navigate to: `https://localhost:7XXX/swagger`

## Step 4: Follow Testing Guide

See `TESTING_GUIDE.md` for detailed testing steps!

---

**Quick Test Sequence:**

1. Register: `POST /api/auth/register`
   ```json
   {
     "username": "player1",
 "password": "Test123!",
     "initialBalance": 1000
   }
   ```

2. Login: `POST /api/auth/login`
   ```json
   {
     "username": "player1",
     "password": "Test123!"
   }
   ```
   ? Copy the `token`

3. Authorize in Swagger:
   - Click "Authorize" button
   - Enter: `Bearer YOUR_TOKEN`
 - Click "Authorize"

4. Play Blackjack: `POST /api/blackjack/deal?apiKey=test-api-key-123`
   ```json
   {
     "betAmount": 50
   }
   ```

5. Hit/Stand/DoubleDown with the returned `gameId`

6. Play Roulette: `POST /api/roulette/spin?apiKey=test-api-key-123`
   ```json
   {
     "bets": [
  {
         "betType": "red",
         "amount": 50,
         "value": ""
       }
     ]
   }
   ```

**Have fun! ??**
