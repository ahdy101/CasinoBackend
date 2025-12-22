import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useGameState } from '../../context/GameStateContext';
import { FaDiceD6 } from 'react-icons/fa';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import './Slots.css';

const Slots = () => {
  const { balance, updateBalance } = useAuth();
  const { updateGameStats } = useGameState();
  const [bet, setBet] = useState(10);
  const [reels, setReels] = useState(['A', 'B', 'C']);
  const [isSpinning, setIsSpinning] = useState(false);
  const [result, setResult] = useState(null);

  const symbols = ['A', 'B', 'C', 'D', 'E', 'F', '7'];

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
        symbols[Math.floor(Math.random() * symbols.length)],
        symbols[Math.floor(Math.random() * symbols.length)],
        symbols[Math.floor(Math.random() * symbols.length)]
      ]);
    }, 100);

    // Stop after 2 seconds
    setTimeout(() => {
      clearInterval(spinInterval);
      const finalReels = [
        symbols[Math.floor(Math.random() * symbols.length)],
        symbols[Math.floor(Math.random() * symbols.length)],
        symbols[Math.floor(Math.random() * symbols.length)]
      ];
      setReels(finalReels);
      setIsSpinning(false);

      let gameResult, winAmount = 0;

      // Check for win
      if (finalReels[0] === finalReels[1] && finalReels[1] === finalReels[2]) {
        gameResult = 'win';
        winAmount = bet * 10;
        updateBalance(winAmount);
        setResult({ type: 'win', message: `Jackpot! You won $${winAmount}!` });
      } else if (finalReels[0] === finalReels[1] || finalReels[1] === finalReels[2]) {
        gameResult = 'win';
        winAmount = bet * 2;
        updateBalance(winAmount);
        setResult({ type: 'win', message: `You won $${winAmount}!` });
      } else {
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
        <h1 className="game-title"><FaDiceD6 /> Slots</h1>
        
        <Card className="game-card-main">
          <div className="slots-machine">
            <div className="slots-reels">
              {reels.map((symbol, index) => (
                <div 
                  key={index}
                  className={`slot-reel ${isSpinning ? 'spinning' : ''}`}
                >
                  {symbol}
                </div>
              ))}
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
            <div className="payout-row">
              <span>3 matching symbols</span>
              <span className="payout-value">10x bet</span>
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

export default Slots;
