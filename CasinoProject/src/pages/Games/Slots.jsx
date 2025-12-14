import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useGameState } from '../../context/GameStateContext';
import { FaDiceD6 } from 'react-icons/fa';
import { MdSave } from 'react-icons/md';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import WarningModal from '../../components/common/WarningModal';
import useGameExitWarning from '../../hooks/useGameExitWarning';
import './Slots.css';

const Slots = () => {
  const { balance, updateBalance } = useAuth();
  const { saveGame, loadGame, deleteGame, hasSavedGame, updateGameStats } = useGameState();
  const [bet, setBet] = useState(10);
  const [reels, setReels] = useState(['A', 'B', 'C']);
  const [isSpinning, setIsSpinning] = useState(false);
  const [result, setResult] = useState(null);
  const [showWarning, setShowWarning] = useState(false);
  const [showResumePrompt, setShowResumePrompt] = useState(false);

  useEffect(() => {
    if (hasSavedGame('slots')) {
      setShowResumePrompt(true);
    }
  }, []);

  const { confirmNavigation, cancelNavigation } = useGameExitWarning(
    isSpinning,
    () => setShowWarning(true)
  );

  const symbols = ['A', 'B', 'C', 'D', 'E', 'F', '7'];

  const handleSaveGame = () => {
    saveGame('slots', { bet, reels });
    setResult({ type: 'info', message: 'Game saved successfully!' });
  };

  const handleLoadGame = () => {
    const saved = loadGame('slots');
    if (saved) {
      setBet(saved.bet);
      setReels(saved.reels);
      setShowResumePrompt(false);
      setResult({ type: 'info', message: 'Game resumed!' });
    }
  };

  const handleNewGame = () => {
    deleteGame('slots');
    setShowResumePrompt(false);
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
              {!isSpinning && (
                <Button variant="warning" onClick={handleSaveGame}>
                  <MdSave /> Save Game
                </Button>
              )}
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

      <WarningModal
        show={showWarning}
        title="Spin in Progress"
        message="The slot machine is currently spinning. If you leave now, you may lose your bet. Are you sure you want to leave?"
        onConfirm={() => {
          setShowWarning(false);
          confirmNavigation();
        }}
        onCancel={() => {
          setShowWarning(false);
          cancelNavigation();
        }}
      />

      <WarningModal
        show={showResumePrompt}
        title="Resume Game?"
        message="You have a saved Slots game. Would you like to resume where you left off?"
        onConfirm={handleLoadGame}
        onCancel={handleNewGame}
        confirmText="Resume"
        cancelText="New Game"
      />
    </div>
  );
};

export default Slots;
