import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { FaDotCircle } from 'react-icons/fa';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import './Roulette.css';

const Roulette = () => {
  const { balance, updateBalance } = useAuth();
  const [bets, setBets] = useState({});
  const [isSpinning, setIsSpinning] = useState(false);
  const [result, setResult] = useState(null);
  const [winningNumber, setWinningNumber] = useState(null);

  const numbers = Array.from({ length: 37 }, (_, i) => i); // 0-36

  const redNumbers = [1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36];

  const placeBet = (position, amount) => {
    if (balance < amount) {
      setResult({ type: 'error', message: 'Insufficient balance!' });
      return;
    }

    setBets(prev => ({
      ...prev,
      [position]: (prev[position] || 0) + amount
    }));
    updateBalance(-amount);
  };

  const getTotalBet = () => {
    return Object.values(bets).reduce((sum, bet) => sum + bet, 0);
  };

  const spin = () => {
    if (getTotalBet() === 0) {
      setResult({ type: 'error', message: 'Please place a bet first!' });
      return;
    }

    setIsSpinning(true);
    setResult(null);

    setTimeout(() => {
      const number = Math.floor(Math.random() * 37);
      setWinningNumber(number);
      setIsSpinning(false);
      calculateWinnings(number);
    }, 3000);
  };

  const calculateWinnings = (number) => {
    let totalWin = 0;

    // Check straight up bet
    if (bets[number]) {
      totalWin += bets[number] * 35;
    }

    // Check red/black
    if (number !== 0) {
      const isRed = redNumbers.includes(number);
      if (bets['red'] && isRed) {
        totalWin += bets['red'] * 2;
      }
      if (bets['black'] && !isRed) {
        totalWin += bets['black'] * 2;
      }

      // Check even/odd
      if (bets['even'] && number % 2 === 0) {
        totalWin += bets['even'] * 2;
      }
      if (bets['odd'] && number % 2 === 1) {
        totalWin += bets['odd'] * 2;
      }

      // Check low/high
      if (bets['low'] && number >= 1 && number <= 18) {
        totalWin += bets['low'] * 2;
      }
      if (bets['high'] && number >= 19 && number <= 36) {
        totalWin += bets['high'] * 2;
      }
    }

    if (totalWin > 0) {
      updateBalance(totalWin);
      setResult({ 
        type: 'win', 
        message: `Number ${number}! You won $${totalWin}!` 
      });
    } else {
      setResult({ 
        type: 'lose', 
        message: `Number ${number}. Better luck next time!` 
      });
    }

    setBets({});
  };

  const clearBets = () => {
    const total = getTotalBet();
    updateBalance(total);
    setBets({});
    setResult(null);
  };

  const getNumberColor = (num) => {
    if (num === 0) return 'green';
    return redNumbers.includes(num) ? 'red' : 'black';
  };

  return (
    <div className="game-container">
      <div className="game-content">
        <h1 className="game-title"><FaDotCircle /> Roulette</h1>
        
        <Card className="game-card-main">
          <div className="roulette-wheel">
            <div className={`wheel ${isSpinning ? 'spinning' : ''}`}>
              <FaDotCircle style={{ fontSize: '4rem' }} />
            </div>
            {winningNumber !== null && !isSpinning && (
              <div className={`winning-number ${getNumberColor(winningNumber)}`}>
                {winningNumber}
              </div>
            )}
          </div>

          {result && (
            <div className={`game-result ${result.type}`}>
              {result.message}
            </div>
          )}

          <div className="betting-grid">
            <h3>Outside Bets</h3>
            <div className="outside-bets">
              <button 
                className="bet-option red"
                onClick={() => placeBet('red', 10)}
                disabled={isSpinning}
              >
                Red
                {bets['red'] && <span className="chip">${bets['red']}</span>}
              </button>
              <button 
                className="bet-option black"
                onClick={() => placeBet('black', 10)}
                disabled={isSpinning}
              >
                Black
                {bets['black'] && <span className="chip">${bets['black']}</span>}
              </button>
              <button 
                className="bet-option"
                onClick={() => placeBet('even', 10)}
                disabled={isSpinning}
              >
                Even
                {bets['even'] && <span className="chip">${bets['even']}</span>}
              </button>
              <button 
                className="bet-option"
                onClick={() => placeBet('odd', 10)}
                disabled={isSpinning}
              >
                Odd
                {bets['odd'] && <span className="chip">${bets['odd']}</span>}
              </button>
              <button 
                className="bet-option"
                onClick={() => placeBet('low', 10)}
                disabled={isSpinning}
              >
                1-18
                {bets['low'] && <span className="chip">${bets['low']}</span>}
              </button>
              <button 
                className="bet-option"
                onClick={() => placeBet('high', 10)}
                disabled={isSpinning}
              >
                19-36
                {bets['high'] && <span className="chip">${bets['high']}</span>}
              </button>
            </div>

            <h3>Inside Bets (Numbers)</h3>
            <div className="numbers-grid">
              {numbers.slice(0, 12).map(num => (
                <button
                  key={num}
                  className={`number-bet ${getNumberColor(num)}`}
                  onClick={() => placeBet(num, 10)}
                  disabled={isSpinning}
                >
                  {num}
                  {bets[num] && <span className="chip">${bets[num]}</span>}
                </button>
              ))}
            </div>
            <p className="bet-info">Click any bet to place $10 chip (35:1 payout on numbers)</p>
          </div>

          <div className="bet-summary">
            <div>Total Bet: <strong>${getTotalBet()}</strong></div>
          </div>

          <div className="action-buttons">
            <Button 
              variant="primary" 
              onClick={spin}
              disabled={isSpinning || getTotalBet() === 0}
            >
              {isSpinning ? 'SPINNING...' : 'SPIN'}
            </Button>
            <Button 
              variant="outline" 
              onClick={clearBets}
              disabled={isSpinning || getTotalBet() === 0}
            >
              Clear Bets
            </Button>
          </div>
        </Card>
      </div>
    </div>
  );
};

export default Roulette;
