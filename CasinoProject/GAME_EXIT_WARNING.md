# Game Exit Warning System

## Overview
A safety feature that prevents users from accidentally losing game progress by warning them when they attempt to navigate away from an active game.

## Features

### 1. Navigation Prevention
- **Internal Navigation**: Intercepts clicks on links within the app
- **Browser Actions**: Warns on refresh, close tab, or back button
- **Modal Warning**: Shows a clear warning dialog before allowing navigation

### 2. Protected Actions
The system activates when:
- **Blackjack**: Game is in 'playing' state (cards dealt but not finished)
- **Slots**: Reels are currently spinning
- **Poker**: Game is in 'playing' state (hand active)
- **Roulette**: Bets are placed OR wheel is spinning

### 3. User Experience
- Clear warning message specific to each game
- Two options: "Stay on Page" (safe) or "Leave Anyway" (proceed)
- Non-intrusive when no game is active
- Prevents accidental data loss

## Implementation

### Components

#### 1. WarningModal Component
```jsx
<WarningModal
  show={showWarning}
  title="Game in Progress"
  message="Custom warning message..."
  onConfirm={() => {/* Leave */}}
  onCancel={() => {/* Stay */}}
/>
```

#### 2. useGameExitWarning Hook
```javascript
const { confirmNavigation, cancelNavigation } = useGameExitWarning(
  isGameActive,  // Boolean: is game currently active?
  () => setShowWarning(true)  // Callback when navigation attempted
);
```

### Usage in Game Components

```jsx
// 1. Import the hook and modal
import WarningModal from '../../components/common/WarningModal';
import useGameExitWarning from '../../hooks/useGameExitWarning';

// 2. Set up state
const [showWarning, setShowWarning] = useState(false);

// 3. Define when game is active
const isGameActive = gameState === 'playing';

// 4. Initialize the hook
const { confirmNavigation, cancelNavigation } = useGameExitWarning(
  isGameActive,
  () => setShowWarning(true)
);

// 5. Add the modal to JSX
<WarningModal
  show={showWarning}
  title="Game in Progress"
  message="Your custom warning message"
  onConfirm={() => {
    setShowWarning(false);
    confirmNavigation();
  }}
  onCancel={() => {
    setShowWarning(false);
    cancelNavigation();
  }}
/>
```

## Technical Details

### Hook Behavior
The `useGameExitWarning` hook provides two types of protection:

1. **Browser Events** (via `beforeunload`)
   - Prevents page refresh
   - Prevents tab close
   - Prevents browser back/forward
   - Shows browser's native warning

2. **React Router Navigation**
   - Intercepts internal link clicks
   - Captures the intended destination
   - Shows custom modal
   - Allows or cancels navigation based on user choice

### Event Flow
```
User clicks link → Hook intercepts → Modal shows
                                    ↓
                           User chooses:
                           ├─ Stay → Navigation cancelled
                           └─ Leave → Navigation proceeds
```

## Files

### Core Files
- `src/components/common/WarningModal.jsx` - Modal component
- `src/components/common/WarningModal.css` - Modal styles
- `src/hooks/useGameExitWarning.js` - Navigation blocking hook

### Updated Game Files
- `src/pages/Games/Blackjack.jsx`
- `src/pages/Games/Slots.jsx`
- `src/pages/Games/Poker.jsx`
- `src/pages/Games/Roulette.jsx`

## Customization

### Game-Specific Messages
Each game has a tailored warning message:

- **Blackjack**: "...your current hand and bet will be lost..."
- **Slots**: "...the slot machine is currently spinning..."
- **Poker**: "...your current hand and bet will be lost..."
- **Roulette**: "...you have active bets or the wheel is spinning..."

### Styling
Customize the modal appearance in `WarningModal.css`:
- Border color (currently red for warnings)
- Animation timing
- Button styles
- Responsive breakpoints

## Best Practices

### When to Activate Warning
✅ **DO activate when:**
- Game round is in progress
- Money is at stake
- Results are pending
- User has made game decisions

❌ **DON'T activate when:**
- User is just viewing the game
- Game round has ended
- Results have been processed
- User is in betting/setup phase only

### Example Conditions
```javascript
// Good - specific condition
const isGameActive = gameState === 'playing';

// Good - multiple conditions
const isGameActive = isSpinning || hasPendingBets;

// Bad - too broad
const isGameActive = true; // Always blocks!

// Bad - too narrow
const isGameActive = false; // Never protects
```

## Future Enhancements

Potential improvements:
- [ ] Auto-save game state before navigation
- [ ] Resume game after returning
- [ ] Session recovery after browser crash
- [ ] More granular state tracking
- [ ] Analytics on exit patterns
- [ ] Timeout warnings for inactive games

## Testing

### Test Scenarios
1. Start a game → Click nav link → Verify warning shows
2. Start a game → Refresh page → Verify browser warning
3. No active game → Click nav link → Verify no warning
4. Active game → Choose "Stay" → Verify navigation cancelled
5. Active game → Choose "Leave" → Verify navigation proceeds
6. Active game → Close tab → Verify browser warning

### Browser Compatibility
- ✅ Chrome/Edge (tested)
- ✅ Firefox (tested)
- ✅ Safari (tested)
- ⚠️ Mobile browsers may show native warnings

## Security Considerations

- Does NOT prevent:
  - Forced browser close
  - System shutdown
  - Network disconnection
  - Tab crash

- Server-side game state management recommended for:
  - Critical transactions
  - High-value bets
  - Tournament play
  - Multiplayer games
