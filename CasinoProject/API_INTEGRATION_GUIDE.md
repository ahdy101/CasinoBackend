# Casino Backend API Integration Guide

## Overview
This document provides complete integration details for connecting the React frontend with the C# .NET backend API. All endpoints follow RESTful conventions and require proper authentication.

---

## Base Configuration

### API Base URL
```javascript
// src/config/api.js
export const API_CONFIG = {
  BASE_URL: process.env.VITE_API_URL || 'https://localhost:7000/api',
  TIMEOUT: 30000,
  HEADERS: {
    'Content-Type': 'application/json',
  }
};
```

### API Client Setup
```javascript
// src/services/apiClient.js
import axios from 'axios';
import { API_CONFIG } from '../config/api';

const apiClient = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT,
  headers: API_CONFIG.HEADERS
});

// Request interceptor - Add JWT token and API key
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('jwtToken');
    const apiKey = localStorage.getItem('apiKey');
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    if (apiKey) {
      config.params = { ...config.params, apiKey };
    }
    
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - Handle errors globally
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Redirect to login
      localStorage.removeItem('jwtToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default apiClient;
```

---

## Authentication Endpoints

### 1. User Registration
**Endpoint:** `POST /api/Users/register`

**Request:**
```javascript
// src/services/authService.js
export const register = async (userData) => {
  try {
    const response = await apiClient.post('/Users/register', {
      username: userData.username,
      email: userData.email,
      password: userData.password
    });
    
    return {
      success: true,
      data: response.data,
      message: 'Registration successful'
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Registration failed',
      errors: error.response?.data?.errors || []
    };
  }
};
```

**Request Body:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "username": "john_doe",
  "email": "john@example.com",
  "balance": 1000.00,
  "createdAt": "2025-12-12T10:30:00Z"
}
```

**Frontend Integration:**
```javascript
// Update AuthContext.jsx
const register = async (email, password, name) => {
  const result = await authService.register({
    username: name,
    email: email,
    password: password
  });
  
  if (result.success) {
    setUser(result.data);
    setBalance(result.data.balance);
    localStorage.setItem('user', JSON.stringify(result.data));
  }
  
  return result;
};
```

---

### 2. User Login
**Endpoint:** `POST /api/Users/login`

**Request:**
```javascript
export const login = async (email, password) => {
  try {
    const response = await apiClient.post('/Users/login', {
      email: email,
      password: password
    });
    
    // Store JWT token and API key
    localStorage.setItem('jwtToken', response.data.token);
    localStorage.setItem('apiKey', response.data.apiKey);
    
    return {
      success: true,
      data: response.data.user,
      token: response.data.token
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Login failed'
    };
  }
};
```

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "apiKey": "tenant_api_key_12345",
  "user": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "balance": 1000.00
  }
}
```

**Frontend Integration:**
```javascript
// Update AuthContext.jsx
const login = async (email, password) => {
  const result = await authService.login(email, password);
  
  if (result.success) {
    setUser(result.data);
    localStorage.setItem('user', JSON.stringify(result.data));
  }
  
  return result;
};
```

---

### 3. Get User Profile
**Endpoint:** `GET /api/Users/{id}?apiKey={apiKey}`

**Request:**
```javascript
export const getUserProfile = async (userId) => {
  try {
    const response = await apiClient.get(`/Users/${userId}`);
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch profile'
    };
  }
};
```

**Response (200 OK):**
```json
{
  "id": 1,
  "username": "john_doe",
  "email": "john@example.com",
  "balance": 1500.50,
  "createdAt": "2025-12-12T10:30:00Z"
}
```

---

### 4. Update Balance
**Endpoint:** `PUT /api/Users/{id}/balance?apiKey={apiKey}`

**Request:**
```javascript
export const updateBalance = async (userId, newBalance) => {
  try {
    const response = await apiClient.put(`/Users/${userId}/balance`, {
      balance: newBalance
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to update balance'
    };
  }
};
```

**Request Body:**
```json
{
  "balance": 2000.00
}
```

---

## Wallet Endpoints

### 5. Get Balance
**Endpoint:** `GET /api/Wallet/balance?apiKey={apiKey}`

**Request:**
```javascript
// src/services/walletService.js
export const getBalance = async () => {
  try {
    const response = await apiClient.get('/Wallet/balance');
    return {
      success: true,
      balance: response.data.balance
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch balance'
    };
  }
};
```

**Response (200 OK):**
```json
{
  "balance": 1500.50,
  "userId": 1
}
```

---

### 6. Add Funds
**Endpoint:** `POST /api/Wallet/add-funds?apiKey={apiKey}`

**Request:**
```javascript
export const addFunds = async (amount) => {
  try {
    const response = await apiClient.post('/Wallet/add-funds', {
      amount: amount
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to add funds'
    };
  }
};
```

**Request Body:**
```json
{
  "amount": 500.00
}
```

**Response (200 OK):**
```json
{
  "newBalance": 2000.50,
  "addedAmount": 500.00,
  "transactionId": "txn_12345"
}
```

---

### 7. Cash Out
**Endpoint:** `POST /api/Wallet/cash-out?apiKey={apiKey}`

**Request:**
```javascript
export const cashOut = async (amount) => {
  try {
    const response = await apiClient.post('/Wallet/cash-out', {
      amount: amount
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to cash out'
    };
  }
};
```

**Request Body:**
```json
{
  "amount": 300.00
}
```

**Response (200 OK):**
```json
{
  "newBalance": 1700.50,
  "withdrawnAmount": 300.00,
  "transactionId": "txn_12346"
}
```

---

### 8. Get Transaction History
**Endpoint:** `GET /api/Wallet/transactions?apiKey={apiKey}`

**Request:**
```javascript
export const getTransactionHistory = async (params = {}) => {
  try {
    const response = await apiClient.get('/Wallet/transactions', {
      params: {
        from: params.from,
        to: params.to,
        limit: params.limit || 50
      }
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch transactions'
    };
  }
};
```

**Response (200 OK):**
```json
{
  "transactions": [
    {
      "id": 1,
      "type": "bet",
      "amount": -50.00,
      "balance": 1450.50,
      "gameType": "blackjack",
      "createdAt": "2025-12-12T14:20:00Z"
    },
    {
      "id": 2,
      "type": "win",
      "amount": 100.00,
      "balance": 1550.50,
      "gameType": "blackjack",
      "createdAt": "2025-12-12T14:22:00Z"
    }
  ],
  "total": 2
}
```

---

## Blackjack Game Endpoints

### 9. Deal New Blackjack Game
**Endpoint:** `POST /api/Blackjack/deal?apiKey={apiKey}`

**Request:**
```javascript
// src/services/blackjackService.js
export const dealNewGame = async (betAmount) => {
  try {
    const response = await apiClient.post('/Blackjack/deal', {
      betAmount: betAmount
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to deal cards'
    };
  }
};
```

**Request Body:**
```json
{
  "betAmount": 50.00
}
```

**Response (200 OK):**
```json
{
  "gameId": 123,
  "playerHand": [
    { "suit": "♠️", "value": "K" },
    { "suit": "♥️", "value": "9" }
  ],
  "dealerHand": [
    { "suit": "♦️", "value": "7" }
  ],
  "playerTotal": 19,
  "dealerTotal": 7,
  "status": "Active",
  "betAmount": 50.00,
  "canHit": true,
  "canStand": true,
  "canDoubleDown": true,
  "canSplit": false
}
```

**Frontend Integration:**
```javascript
// Update Blackjack.jsx
const startGame = async () => {
  if (balance < bet) {
    setResult({ type: 'error', message: 'Insufficient balance!' });
    return;
  }

  const result = await blackjackService.dealNewGame(bet);
  
  if (result.success) {
    setGameId(result.data.gameId);
    setPlayerHand(result.data.playerHand);
    setDealerHand(result.data.dealerHand);
    setGameState('playing');
    updateBalance(-bet); // Update local balance
  } else {
    setResult({ type: 'error', message: result.message });
  }
};
```

---

### 10. Hit Action
**Endpoint:** `POST /api/Blackjack/hit?apiKey={apiKey}`

**Request:**
```javascript
export const hit = async (gameId) => {
  try {
    const response = await apiClient.post('/Blackjack/hit', {
      gameId: gameId
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Hit action failed'
    };
  }
};
```

**Request Body:**
```json
{
  "gameId": 123
}
```

**Response (200 OK):**
```json
{
  "gameId": 123,
  "playerHand": [
    { "suit": "♠️", "value": "K" },
    { "suit": "♥️", "value": "9" },
    { "suit": "♣️", "value": "5" }
  ],
  "dealerHand": [
    { "suit": "♦️", "value": "7" }
  ],
  "playerTotal": 24,
  "dealerTotal": 7,
  "status": "PlayerBust",
  "betAmount": 50.00,
  "payout": 0.00,
  "canHit": false,
  "canStand": false
}
```

---

### 11. Stand Action
**Endpoint:** `POST /api/Blackjack/stand?apiKey={apiKey}`

**Request:**
```javascript
export const stand = async (gameId) => {
  try {
    const response = await apiClient.post('/Blackjack/stand', {
      gameId: gameId
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Stand action failed'
    };
  }
};
```

**Response (200 OK):**
```json
{
  "gameId": 123,
  "playerHand": [
    { "suit": "♠️", "value": "K" },
    { "suit": "♥️", "value": "9" }
  ],
  "dealerHand": [
    { "suit": "♦️", "value": "7" },
    { "suit": "♠️", "value": "8" },
    { "suit": "♣️", "value": "3" }
  ],
  "playerTotal": 19,
  "dealerTotal": 18,
  "status": "PlayerWin",
  "betAmount": 50.00,
  "payout": 100.00,
  "canHit": false,
  "canStand": false
}
```

---

### 12. Double Down Action
**Endpoint:** `POST /api/Blackjack/double-down?apiKey={apiKey}`

**Request:**
```javascript
export const doubleDown = async (gameId) => {
  try {
    const response = await apiClient.post('/Blackjack/double-down', {
      gameId: gameId
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Double down failed'
    };
  }
};
```

---

## Poker Game Endpoints

### 13. Initialize Poker Table
**Endpoint:** `POST /api/Poker/initialize?apiKey={apiKey}`

**Request:**
```javascript
// src/services/pokerService.js
export const initializeTable = async (buyIn) => {
  try {
    const response = await apiClient.post('/Poker/initialize', {
      buyIn: buyIn,
      gameType: 'TexasHoldem'
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to initialize table'
    };
  }
};
```

**Request Body:**
```json
{
  "buyIn": 100.00,
  "gameType": "TexasHoldem"
}
```

**Response (200 OK):**
```json
{
  "tableId": 456,
  "gameType": "TexasHoldem",
  "buyIn": 100.00,
  "pot": 0.00,
  "status": "WaitingForPlayers"
}
```

---

### 14. Deal Poker Cards
**Endpoint:** `POST /api/Poker/deal?apiKey={apiKey}`

**Request:**
```javascript
export const dealPokerCards = async (tableId) => {
  try {
    const response = await apiClient.post('/Poker/deal', {
      tableId: tableId
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to deal cards'
    };
  }
};
```

**Response (200 OK):**
```json
{
  "tableId": 456,
  "playerHand": [
    { "suit": "♠️", "value": "A" },
    { "suit": "♥️", "value": "K" }
  ],
  "communityCards": [],
  "pot": 30.00,
  "currentBet": 0.00,
  "stage": "PreFlop",
  "canCheck": true,
  "canBet": true,
  "canFold": true
}
```

---

### 15. Poker Action (Check/Bet/Fold/Raise)
**Endpoint:** `POST /api/Poker/action?apiKey={apiKey}`

**Request:**
```javascript
export const pokerAction = async (tableId, action, amount = null) => {
  try {
    const response = await apiClient.post('/Poker/action', {
      tableId: tableId,
      action: action, // "Check", "Bet", "Fold", "Raise"
      amount: amount
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Action failed'
    };
  }
};
```

**Request Body:**
```json
{
  "tableId": 456,
  "action": "Bet",
  "amount": 50.00
}
```

**Response (200 OK):**
```json
{
  "tableId": 456,
  "playerHand": [
    { "suit": "♠️", "value": "A" },
    { "suit": "♥️", "value": "K" }
  ],
  "communityCards": [
    { "suit": "♦️", "value": "Q" },
    { "suit": "♣️", "value": "J" },
    { "suit": "♠️", "value": "10" }
  ],
  "pot": 130.00,
  "currentBet": 50.00,
  "stage": "Flop",
  "yourAction": "Bet",
  "amountBet": 50.00
}
```

---

## Roulette Game Endpoints

### 16. Spin Roulette
**Endpoint:** `POST /api/Roulette/spin?apiKey={apiKey}`

**Request:**
```javascript
// src/services/rouletteService.js
export const spinRoulette = async (bets) => {
  try {
    const response = await apiClient.post('/Roulette/spin', {
      bets: bets
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Spin failed'
    };
  }
};
```

**Request Body:**
```json
{
  "bets": [
    {
      "type": "straight",
      "position": 17,
      "amount": 10.00
    },
    {
      "type": "color",
      "position": "red",
      "amount": 50.00
    },
    {
      "type": "evenOdd",
      "position": "even",
      "amount": 25.00
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "winningNumber": 17,
  "color": "black",
  "bets": [
    {
      "type": "straight",
      "position": 17,
      "amount": 10.00,
      "payout": 350.00,
      "won": true
    },
    {
      "type": "color",
      "position": "red",
      "amount": 50.00,
      "payout": 0.00,
      "won": false
    },
    {
      "type": "evenOdd",
      "position": "even",
      "amount": 25.00,
      "payout": 0.00,
      "won": false
    }
  ],
  "totalBet": 85.00,
  "totalPayout": 350.00,
  "netWin": 265.00,
  "newBalance": 1765.50
}
```

**Frontend Integration:**
```javascript
// Update Roulette.jsx
const spin = async () => {
  if (getTotalBet() === 0) {
    setResult({ type: 'error', message: 'Please place a bet first!' });
    return;
  }

  setIsSpinning(true);
  setResult(null);

  // Convert local bets to API format
  const apiBets = Object.entries(bets).map(([position, amount]) => ({
    type: getRouletteType(position),
    position: position,
    amount: amount
  }));

  const result = await rouletteService.spinRoulette(apiBets);
  
  if (result.success) {
    setWinningNumber(result.data.winningNumber);
    setIsSpinning(false);
    
    if (result.data.netWin > 0) {
      setResult({ 
        type: 'win', 
        message: `🎉 Number ${result.data.winningNumber}! You won $${result.data.netWin}!` 
      });
      updateBalance(result.data.netWin);
    } else {
      setResult({ 
        type: 'lose', 
        message: `Number ${result.data.winningNumber}. Better luck next time!` 
      });
    }
    
    setBets({});
  } else {
    setIsSpinning(false);
    setResult({ type: 'error', message: result.message });
  }
};
```

---

## Slots Game Endpoints

### 17. Spin Slots
**Endpoint:** `POST /api/Slots/spin?apiKey={apiKey}`

**Request:**
```javascript
// src/services/slotsService.js
export const spinSlots = async (betAmount) => {
  try {
    const response = await apiClient.post('/Slots/spin', {
      betAmount: betAmount
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Spin failed'
    };
  }
};
```

**Request Body:**
```json
{
  "betAmount": 25.00
}
```

**Response (200 OK):**
```json
{
  "reels": ["🍒", "🍒", "🍒"],
  "betAmount": 25.00,
  "payout": 250.00,
  "multiplier": 10,
  "result": "Jackpot",
  "newBalance": 1725.50
}
```

**Frontend Integration:**
```javascript
// Update Slots.jsx
const handleSpin = async () => {
  if (balance < bet) {
    setResult({ type: 'error', message: 'Insufficient balance!' });
    return;
  }

  setIsSpinning(true);
  setResult(null);
  updateBalance(-bet); // Optimistic update

  const result = await slotsService.spinSlots(bet);
  
  if (result.success) {
    // Animate reels
    const spinInterval = setInterval(() => {
      setReels([
        symbols[Math.floor(Math.random() * symbols.length)],
        symbols[Math.floor(Math.random() * symbols.length)],
        symbols[Math.floor(Math.random() * symbols.length)]
      ]);
    }, 100);

    setTimeout(() => {
      clearInterval(spinInterval);
      setReels(result.data.reels);
      setIsSpinning(false);

      if (result.data.payout > 0) {
        updateBalance(result.data.payout);
        setResult({ 
          type: 'win', 
          message: `🎉 ${result.data.result}! You won $${result.data.payout}!` 
        });
      } else {
        setResult({ type: 'lose', message: 'Try again!' });
      }
    }, 2000);
  } else {
    setIsSpinning(false);
    setResult({ type: 'error', message: result.message });
  }
};
```

---

## Game History Endpoints

### 18. Get Game History
**Endpoint:** `GET /api/GameHistory?apiKey={apiKey}`

**Request:**
```javascript
// src/services/gameHistoryService.js
export const getGameHistory = async (params = {}) => {
  try {
    const response = await apiClient.get('/GameHistory', {
      params: {
        gameType: params.gameType,
        from: params.from,
        to: params.to,
        limit: params.limit || 50
      }
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch history'
    };
  }
};
```

**Response (200 OK):**
```json
{
  "games": [
    {
      "id": 1,
      "gameType": "blackjack",
      "betAmount": 50.00,
      "payout": 100.00,
      "result": "win",
      "createdAt": "2025-12-12T14:20:00Z"
    },
    {
      "id": 2,
      "gameType": "roulette",
      "betAmount": 85.00,
      "payout": 350.00,
      "result": "win",
      "createdAt": "2025-12-12T14:25:00Z"
    }
  ],
  "total": 2,
  "totalBet": 135.00,
  "totalPayout": 450.00,
  "netProfit": 315.00
}
```

---

### 19. Get Player Statistics
**Endpoint:** `GET /api/GameHistory/stats?apiKey={apiKey}`

**Request:**
```javascript
export const getPlayerStats = async () => {
  try {
    const response = await apiClient.get('/GameHistory/stats');
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to fetch stats'
    };
  }
};
```

**Response (200 OK):**
```json
{
  "totalGames": 150,
  "totalBet": 7500.00,
  "totalWon": 8250.00,
  "netProfit": 750.00,
  "winRate": 52.5,
  "favoriteGame": "blackjack",
  "biggestWin": 500.00,
  "gameBreakdown": {
    "blackjack": { "games": 60, "winRate": 55.0 },
    "poker": { "games": 40, "winRate": 50.0 },
    "roulette": { "games": 30, "winRate": 48.0 },
    "slots": { "games": 20, "winRate": 52.0 }
  }
}
```

---

## AI Model Endpoints (Optional)

### 20. Get AI Blackjack Recommendation
**Endpoint:** `POST /api/AI/blackjack/predict?apiKey={apiKey}`

**Request:**
```javascript
// src/services/aiService.js
export const getBlackjackRecommendation = async (gameState) => {
  try {
    const response = await apiClient.post('/AI/blackjack/predict', {
      playerTotal: gameState.playerTotal,
      dealerUpCard: gameState.dealerUpCard,
      isSoftHand: gameState.isSoftHand,
      canDoubleDown: gameState.canDoubleDown,
      canSplit: gameState.canSplit
    });
    return {
      success: true,
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: error.response?.data?.message || 'Failed to get recommendation'
    };
  }
};
```

**Request Body:**
```json
{
  "playerTotal": 16,
  "dealerUpCard": 10,
  "isSoftHand": false,
  "canDoubleDown": false,
  "canSplit": false
}
```

**Response (200 OK):**
```json
{
  "recommendedAction": "Hit",
  "confidence": 0.87,
  "expectedValue": -0.15,
  "alternativeActions": [
    {
      "action": "Stand",
      "expectedValue": -0.48,
      "probability": 0.13
    }
  ]
}
```

---

## Error Handling Patterns

### Standard Error Response
```json
{
  "success": false,
  "message": "Insufficient balance",
  "errors": [
    "Required: 50.00, Available: 25.00"
  ],
  "timestamp": "2025-12-12T14:30:00Z"
}
```

### Frontend Error Handler
```javascript
// src/utils/errorHandler.js
export const handleApiError = (error, showNotification) => {
  const message = error.response?.data?.message || 
                  error.message || 
                  'An unexpected error occurred';
  
  const errors = error.response?.data?.errors || [];
  
  showNotification({
    type: 'error',
    title: 'Error',
    message: message,
    details: errors.join(', ')
  });
  
  // Log to console in development
  if (process.env.NODE_ENV === 'development') {
    console.error('API Error:', error);
  }
};
```

---

## Complete Service Example

### Comprehensive Blackjack Service
```javascript
// src/services/blackjackService.js
import apiClient from './apiClient';
import { handleApiError } from '../utils/errorHandler';

class BlackjackService {
  async dealNewGame(betAmount) {
    try {
      const response = await apiClient.post('/Blackjack/deal', {
        betAmount: betAmount
      });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to deal cards',
        errors: error.response?.data?.errors || []
      };
    }
  }

  async hit(gameId) {
    try {
      const response = await apiClient.post('/Blackjack/hit', {
        gameId: gameId
      });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Hit action failed'
      };
    }
  }

  async stand(gameId) {
    try {
      const response = await apiClient.post('/Blackjack/stand', {
        gameId: gameId
      });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Stand action failed'
      };
    }
  }

  async doubleDown(gameId) {
    try {
      const response = await apiClient.post('/Blackjack/double-down', {
        gameId: gameId
      });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Double down failed'
      };
    }
  }

  async split(gameId) {
    try {
      const response = await apiClient.post('/Blackjack/split', {
        gameId: gameId
      });
      return {
        success: true,
        data: response.data
      };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Split action failed'
      };
    }
  }
}

export default new BlackjackService();
```

---

## Environment Configuration

### .env.local
```env
VITE_API_URL=https://localhost:7000/api
VITE_API_TIMEOUT=30000
VITE_ENABLE_AI_FEATURES=false
```

### .env.production
```env
VITE_API_URL=https://api.thesiverslayed.com/api
VITE_API_TIMEOUT=30000
VITE_ENABLE_AI_FEATURES=true
```

---

## Testing API Integration

### API Test Utility
```javascript
// src/utils/apiTest.js
export const testApiConnection = async () => {
  try {
    const response = await apiClient.get('/health');
    return {
      success: true,
      message: 'API connection successful',
      data: response.data
    };
  } catch (error) {
    return {
      success: false,
      message: 'API connection failed',
      error: error.message
    };
  }
};
```

---

## Summary Checklist

### Backend Requirements
- [ ] .NET 10.0 Web API project created
- [ ] Entity Framework Core 9.0 configured with MySQL
- [ ] JWT authentication implemented
- [ ] Multi-tenancy with API keys
- [ ] All game engines (Blackjack, Poker, Roulette, Slots)
- [ ] Wallet service with transaction support
- [ ] BCrypt password hashing
- [ ] Swagger/OpenAPI documentation
- [ ] CORS configured for React frontend

### Frontend Integration
- [ ] API client with axios configured
- [ ] Request/response interceptors
- [ ] JWT token management
- [ ] API key handling
- [ ] Service layer for each game
- [ ] Error handling utility
- [ ] Environment variables
- [ ] Loading states and error messages
- [ ] Real-time balance updates
- [ ] Transaction history display

---

**Next Steps:**
1. Install Node.js (download from https://nodejs.org/)
2. Set up the .NET backend following `webapi.instructions.md`
3. Configure connection strings and JWT settings
4. Run database migrations
5. Update `VITE_API_URL` in React `.env.local`
6. Test API endpoints with Postman/Swagger
7. Integrate services into React components
8. Test end-to-end game flows

This guide provides complete integration between your React frontend and C# .NET backend!
