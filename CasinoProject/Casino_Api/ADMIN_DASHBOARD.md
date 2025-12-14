# Admin Dashboard

## Overview
The Admin Dashboard provides comprehensive monitoring and management capabilities for The Silver Slayed casino platform.

## Access
- **URL**: `/admin`
- **Login**: Use admin credentials
  - Email: `admin@silverslayed.com`
  - Any password (for development)
- **Access Control**: Protected route - only accessible to users with admin role

## Features

### 1. User Statistics
- **Total Users**: Count of all registered users
- **Active Users**: Users who have played in the last 7 days
- **New Signups**: Daily, weekly, and monthly signup counts
- **Recent Users**: List of the 10 most recent registrations with:
  - Username, email, balance
  - Join date and last activity

### 2. Transaction Statistics
- **Total Volume**: Sum of all transaction amounts
- **Period Volumes**: Today, this week, this month
- **Transaction Counts**: Total and daily transaction counts
- **Average Transaction**: Mean transaction amount
- **Recent Transactions**: Last 20 transactions with:
  - User, game type, bet amount, win amount, timestamp

### 3. Game Statistics
- **Games Played**: Total and period-based (today, week, month)
- **Total Wagered**: Sum of all bet amounts
- **Total Won**: Sum of all win amounts
- **House Edge**: Calculated profit margin percentage
- **Game Type Breakdown**: Statistics per game type:
  - Games played, total wagered, total won, win rate

## API Endpoints

### Get Complete Dashboard Data
```
GET /api/admin/dashboard
```
Returns all statistics in a single response.

### Get User Statistics Only
```
GET /api/admin/users
```
Returns detailed user statistics.

### Get Transaction Statistics Only
```
GET /api/admin/transactions
```
Returns detailed transaction statistics.

### Get Game Statistics Only
```
GET /api/admin/games
```
Returns detailed game statistics.

## Implementation Details

### Backend (C# .NET)
- **Controller**: `AdminController.cs`
- **DTOs**: `AdminResponses.cs`
- **Database**: Uses existing `AppDbContext`
- **Environment**: Reads from configuration (no hardcoded credentials)

### Frontend (React)
- **Component**: `AdminDashboard.jsx`
- **Styles**: `AdminDashboard.css`
- **Route Protection**: `AdminRoute` component in `App.jsx`
- **Authentication**: Integrated with `AuthContext`

## Security Notes

⚠️ **Current Implementation**:
- Admin check is based on email match (development only)
- No actual authentication middleware on backend endpoints
- Routes are protected on frontend only

✅ **Production Requirements**:
1. Implement proper JWT-based authentication
2. Add `[Authorize(Roles = "Admin")]` attribute to controller
3. Add admin role management in database
4. Implement proper password hashing for admin users
5. Add audit logging for admin actions
6. Add rate limiting to prevent abuse

## Future Enhancements

### Planned Features
- [ ] Real-time dashboard updates (WebSocket/SignalR)
- [ ] User management (ban, suspend, modify balance)
- [ ] Transaction filtering and search
- [ ] Export data to CSV/Excel
- [ ] Game configuration management
- [ ] System health monitoring
- [ ] Audit log viewer
- [ ] Advanced analytics and charts
- [ ] Email notification system
- [ ] Report generation

### Data Visualization
- [ ] Add Chart.js or Recharts for graphs
- [ ] Revenue trend charts
- [ ] User growth charts
- [ ] Game popularity pie charts
- [ ] Win/loss distribution

## Usage

### Development
1. Start the backend API
2. Start the frontend dev server
3. Login with admin credentials
4. Navigate to `/admin`

### Testing
- Ensure database has sample data for meaningful statistics
- Test with different date ranges
- Verify calculations (house edge, win rates, etc.)

## Database Schema Used

### Tables
- `Users` - User accounts
- `Bets` - All game bets and outcomes
- `BlackjackGames` - Blackjack game sessions
- `GameHistories` - General game history

### Key Fields
- User: Id, Username, Email, Balance, CreatedAt
- Bet: Amount, WinAmount, GameType, CreatedAt, UserId

## Configuration

All database connections use environment variables from `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  }
}
```

No credentials are hardcoded in the application code.
