# Casino Backend - API Testing Guide ??

## Quick Start

### 1. Setup Database
```bash
cd Casino_Api
dotnet ef database update
```

### 2. Create Test API Key
Run this SQL in your MySQL database:
```sql
USE casino_db;

INSERT INTO TenantApiKeys (TenantName, ApiKey, CreatedAt)
VALUES ('TestTenant', 'test-api-key-123', UTC_TIMESTAMP());
```

### 3. Run the API
```bash
cd Casino_Api
dotnet run
```

The API will start at: `https://localhost:XXXXX` (check console for port)

### 4. Open Swagger UI
Navigate to: `https://localhost:XXXXX/swagger`

---

## Testing Workflow

### Step 1: Register a User
**Endpoint:** `POST /api/auth/register`

**Request:**
```json
{
  "username": "testplayer",
  "password": "Test123!",
  "initialBalance": 1000
}
```

**Expected Response:** `201 Created`
```json
{
  "id": 1,
  "username": "testplayer",
  "balance": 1000,
  "createdAt": "2025-01-22T..."
}
```

---

### Step 2: Login
**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "username": "testplayer",
  "password": "Test123!"
}
```

**Expected Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "testplayer"
}
```

**?? IMPORTANT:** Copy the `token` value - you'll need it for all game endpoints!

---

### Step 3: Authorize in Swagger
1. Click the **"Authorize"** button at the top of Swagger UI
2. Enter: `Bearer YOUR_TOKEN_HERE` (replace YOUR_TOKEN_HERE with the actual token)
3. Click **"Authorize"**
4. Click **"Close"**

---

### Step 4: Test Blackjack

#### Deal Cards
**Endpoint:** `POST /api/blackjack/deal`

**Query Param:** `apiKey=test-api-key-123`

**Request:**
```json
{
  "betAmount": 50
}
```

**Expected Response:** `200 OK`
```json
{
  "gameId": 1,
  "playerHand": [
    { "suit": "Hearts", "rank": "K", "value": 10 },
    { "suit": "Diamonds", "rank": "7", "value": 7 }
  ],
  "dealerHand": [
    { "suit": "Spades", "rank": "A", "value": 11 }
  ],
  "playerTotal": 17,
  "dealerTotal": 11,
  "dealerShowsAll": false,
  "status": "Active",
  "betAmount": 50,
  "payout": null,
  "canHit": true,
  "canStand": true,
  "canDoubleDown": true,
  "canSplit": false
}
```

**?? Note the `gameId` - you'll need it for the next actions!**

---

#### Hit (Draw a Card)
**Endpoint:** `POST /api/blackjack/hit`

**Query Param:** `apiKey=test-api-key-123`

**Request:**
```json
{
  "gameId": 1
}
```

**Expected Response:** Player hand updated with new card

---

#### Stand (End Turn)
**Endpoint:** `POST /api/blackjack/stand`

**Query Param:** `apiKey=test-api-key-123`

**Request:**
```json
{
  "gameId": 1
}
```

**Expected Response:** Game completed with payout
```json
{
  "gameId": 1,
  "playerHand": [...],
  "dealerHand": [...],  // Now shows all cards
  "playerTotal": 17,
  "dealerTotal": 18,
  "dealerShowsAll": true,
  "status": "DealerWin",
  "betAmount": 50,
  "payout": 0,  // Lost
  "canHit": false,
  "canStand": false,
  "canDoubleDown": false,
  "canSplit": false
}
```

---

### Step 5: Test Roulette

**Endpoint:** `POST /api/roulette/spin`

**Query Param:** `apiKey=test-api-key-123`

**Request (Multiple Bets):**
```json
{
  "bets": [
    {
      "betType": "red",
      "amount": 50,
      "value": ""
    },
    {
      "betType": "straight",
 "amount": 10,
      "value": "17"
    },
    {
  "betType": "even",
      "amount": 25,
      "value": ""
    }
  ]
}
```

**Expected Response:** `200 OK`
```json
{
  "winningNumber": 17,
  "winningColor": "Black",
  "betResults": [
    {
      "betType": "red",
      "amount": 50,
      "won": false,
      "payout": 0
    },
    {
      "betType": "straight",
      "amount": 10,
      "won": true,
      "payout": 360
    },
    {
      "betType": "even",
 "amount": 25,
      "won": false,
      "payout": 0
    }
  ],
  "totalPayout": 360,
  "newBalance": 1275
}
```

---

## Roulette Bet Types

### Inside Bets (Higher Payouts)
| Bet Type | Description | Payout | Example |
|----------|-------------|--------|---------|
| `straight` | Single number | 35:1 | `"value": "17"` |
| `split` | Two adjacent numbers | 17:1 | Not implemented |
| `street` | Three numbers in a row | 11:1 | Not implemented |
| `corner` | Four numbers | 8:1 | Not implemented |

### Outside Bets (Lower Payouts, Higher Win Rate)
| Bet Type | Description | Payout | Example |
|----------|-------------|--------|---------|
| `red` | Red numbers | 1:1 | `"value": ""` |
| `black` | Black numbers | 1:1 | `"value": ""` |
| `even` | Even numbers | 1:1 | `"value": ""` |
| `odd` | Odd numbers | 1:1 | `"value": ""` |
| `low` | 1-18 | 1:1 | `"value": ""` |
| `high` | 19-36 | 1:1 | `"value": ""` |
| `dozen1` | 1-12 | 2:1 | `"value": ""` |
| `dozen2` | 13-24 | 2:1 | `"value": ""` |
| `dozen3` | 25-36 | 2:1 | `"value": ""` |
| `column1` | 1,4,7...34 | 2:1 | `"value": ""` |
| `column2` | 2,5,8...35 | 2:1 | `"value": ""` |
| `column3` | 3,6,9...36 | 2:1 | `"value": ""` |

---

## Blackjack Game Statuses

- **Active** - Game in progress, player can hit/stand
- **PlayerBust** - Player total > 21, player loses
- **DealerBust** - Dealer total > 21, player wins
- **PlayerBlackjack** - Player has 21 with 2 cards, pays 3:2
- **PlayerWin** - Player total > dealer total
- **DealerWin** - Dealer total > player total
- **Push** - Tie, bet returned

---

## Common Issues & Solutions

### ? "Invalid or missing API key"
**Solution:** Add `?apiKey=test-api-key-123` to the URL

### ? "401 Unauthorized"
**Solution:** 
1. Make sure you've logged in
2. Click "Authorize" in Swagger
3. Enter `Bearer YOUR_TOKEN` (with the word "Bearer" and a space)

### ? "Insufficient funds"
**Solution:** Check your balance with `GET /api/users/{id}?apiKey=test-api-key-123`

### ? "Game not found"
**Solution:** Make sure you're using the correct `gameId` from the deal response

---

## Verify Your Balance

**Endpoint:** `GET /api/users/{id}`

**Query Param:** `apiKey=test-api-key-123`

Replace `{id}` with your user ID (usually `1` for first user)

**Expected Response:**
```json
{
  "id": 1,
  "username": "testplayer",
  "balance": 950,  // Updated after bets
  "createdAt": "2025-01-22T..."
}
```

---

## Testing Checklist

- [ ] User registration works
- [ ] User login returns JWT token
- [ ] Blackjack deal deducts bet from balance
- [ ] Blackjack hit draws a card
- [ ] Blackjack stand completes game and pays out
- [ ] Blackjack double down doubles bet and draws one card
- [ ] Roulette spin with multiple bets works
- [ ] Roulette payout calculations are correct
- [ ] Balance updates after each game
- [ ] API key validation works on all endpoints
- [ ] JWT authorization works on game endpoints

---

## Advanced Testing

### Test Double Down
1. Deal a hand: `POST /api/blackjack/deal` with `betAmount: 100`
2. If you have exactly 2 cards, double down: `POST /api/blackjack/doubledown`
3. Game auto-stands after drawing one card
4. Bet amount is now 200

### Test Immediate Blackjack
1. Deal cards until you get blackjack (Ace + 10-value card)
2. Game automatically completes
3. Payout should be 2.5x your bet (3:2 payout)

### Test Player Bust
1. Deal cards
2. Keep hitting until your total > 21
3. Game automatically ends with `PlayerBust` status
4. Payout is 0

---

## Postman Collection (Alternative to Swagger)

Create these requests in Postman:

### 1. Register User
```
POST https://localhost:PORT/api/auth/register
Body (JSON):
{
  "username": "player2",
  "password": "Test123!",
  "initialBalance": 5000
}
```

### 2. Login
```
POST https://localhost:PORT/api/auth/login
Body (JSON):
{
  "username": "player2",
  "password": "Test123!"
}

Save the token from response!
```

### 3. Play Blackjack
```
POST https://localhost:PORT/api/blackjack/deal?apiKey=test-api-key-123
Headers:
  Authorization: Bearer YOUR_TOKEN
Body (JSON):
{
  "betAmount": 100
}
```

---

## ?? Happy Testing!

If everything works correctly, you now have a fully functional casino backend with:
- ? Secure user authentication
- ? Multi-tenant API key system
- ? Complete Blackjack game
- ? Complete Roulette game
- ? Atomic wallet transactions
- ? Provably fair random number generation

**Good luck at the tables! ??????????**
