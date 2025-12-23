import React, { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useGameState } from '../../context/GameStateContext';
import { FaDiceD6 } from 'react-icons/fa';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import { ATLAS } from '../../constants/images';
import './BeatAtlas.css';

const BeatAtlas = () => {
  const { balance, updateBalance, isAuthenticated, isAdmin } = useAuth();
  const { updateGameStats } = useGameState();
  const [bet, setBet] = useState(10);
  const [reels, setReels] = useState(['CHR', 'LEM', 'STR']);
  const [isSpinning, setIsSpinning] = useState(false);
  const [result, setResult] = useState(null);

  // Redirect admin away from games
  if (isAdmin) {
    return <Navigate to="/admin" replace />;
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Slot symbols with their display representation and payout multipliers
  const symbols = [
    { id: 'cherry', display: 'CHR', payout: 5 },
    { id: 'lemon', display: 'LEM', payout: 5 },
    { id: 'orange', display: 'ORG', payout: 5 },
    { id: 'grape', display: 'GRP', payout: 5 },
    { id: 'watermelon', display: 'WTR', payout: 6 },
    { id: 'star', display: 'STR', payout: 8 },
    { id: 'bell', display: 'BEL', payout: 10 },
    { id: 'diamond', display: 'DMD', payout: 12 },
    { id: 'clover', display: 'CLV', payout: 15 },
    { id: 'atlas', display: 'atlas', payout: 20, isImage: true },
    { id: 'seven', display: 'SVN', payout: 25 }
  ];

  const getRandomSymbol = () => {
    return symbols[Math.floor(Math.random() * symbols.length)];
  };

  const handleSpin = () => {
    if (balance < bet) {
      setResult({ type: 'error', message: 'Insufficient balance!' });
      return;
    }

    setIsSpinning(true);
    setResult(null);
    updateBalance(-bet);

    // Animate spin
    const spinInterval = setInterval(() => {
      setReels([
        getRandomSymbol().display,
        getRandomSymbol().display,
        getRandomSymbol().display
      ]);
    }, 100);

    // Stop after 2 seconds
    setTimeout(() => {
      clearInterval(spinInterval);
      const finalReels = [
        getRandomSymbol(),
        getRandomSymbol(),
        getRandomSymbol()
      ];
      setReels(finalReels.map(s => s.display));
      setIsSpinning(false);

      let gameResult, winAmount = 0;

      // Check for win - all 3 matching
      if (finalReels[0].id === finalReels[1].id && finalReels[1].id === finalReels[2].id) {
        gameResult = 'win';
        winAmount = bet * finalReels[0].payout;
        updateBalance(winAmount);
        const symbolName = finalReels[0].id === 'atlas' ? 'ATLAS' : finalReels[0].display;
        setResult({ type: 'win', message: `JACKPOT! ${symbolName} x3! You won $${winAmount}!` });
      } 
      // Check for 2 matching
      else if (finalReels[0].id === finalReels[1].id || finalReels[1].id === finalReels[2].id || finalReels[0].id === finalReels[2].id) {
        const matchedSymbol = finalReels[0].id === finalReels[1].id ? finalReels[0] : 
                             finalReels[1].id === finalReels[2].id ? finalReels[1] : finalReels[0];
        gameResult = 'win';
        winAmount = bet * 2;
        updateBalance(winAmount);
        setResult({ type: 'win', message: `You won $${winAmount}!` });
      } 
      else {
        gameResult = 'loss';
        setResult({ type: 'lose', message: 'Try again!' });
      }

      // Update statistics
      updateGameStats('slots', gameResult, bet, winAmount);
    }, 2000);
  };

  return (
    <div className="game-container">
      <div className="game-content">
        <h1 className="game-title"><FaDiceD6 /> Beat Atlas</h1>
        
        <Card className="game-card-main">
          <div className="slots-machine">
            <div className="slots-reels">
              {reels.map((symbolDisplay, index) => {
                const symbolObj = symbols.find(s => s.display === symbolDisplay);
                return (
                  <div 
                    key={index}
                    className={`slot-reel ${isSpinning ? 'spinning' : ''}`}
                  >
                    {symbolObj?.isImage ? (
                      <img 
                        src={ATLAS} 
                        alt="Atlas" 
                        className="symbol-image"
                      />
                    ) : (
                      <span className="symbol-emoji">{symbolDisplay}</span>
                    )}
                  </div>
                );
              })}
            </div>
          </div>

          {result && (
            <div className={`game-result ${result.type}`}>
              {result.message}
            </div>
          )}

          <div className="betting-controls">
            <div className="bet-selector">
              <label>Bet Amount</label>
              <div className="bet-buttons">
                {[10, 25, 50, 100].map(amount => (
                  <button
                    key={amount}
                    className={`bet-button ${bet === amount ? 'active' : ''}`}
                    onClick={() => setBet(amount)}
                    disabled={isSpinning}
                  >
                    ${amount}
                  </button>
                ))}
              </div>
            </div>

            <div className="action-buttons">
              <Button
                variant="primary"
                size="large"
                fullWidth
                onClick={handleSpin}
                disabled={isSpinning || balance < bet}
              >
                {isSpinning ? 'SPINNING...' : 'SPIN'}
              </Button>
            </div>
          </div>

          <div className="payout-table">
            <h3>Payout Table</h3>
            <div className="payout-row highlight">
              <span>SVN SVN SVN</span>
              <span className="payout-value">25x bet</span>
            </div>
            <div className="payout-row highlight">
              <span>ATLAS x3</span>
              <span className="payout-value">20x bet</span>
            </div>
            <div className="payout-row">
              <span>CLV CLV CLV</span>
              <span className="payout-value">15x bet</span>
            </div>
            <div className="payout-row">
              <span>DMD DMD DMD</span>
              <span className="payout-value">12x bet</span>
            </div>
            <div className="payout-row">
              <span>BEL BEL BEL</span>
              <span className="payout-value">10x bet</span>
            </div>
            <div className="payout-row">
              <span>STR STR STR</span>
              <span className="payout-value">8x bet</span>
            </div>
            <div className="payout-row">
              <span>WTR WTR WTR</span>
              <span className="payout-value">6x bet</span>
            </div>
            <div className="payout-row">
              <span>CHR LEM ORG (any fruit x3)</span>
              <span className="payout-value">5x bet</span>
            </div>
            <div className="payout-row">
              <span>2 matching symbols</span>
              <span className="payout-value">2x bet</span>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
};

export default BeatAtlas;
