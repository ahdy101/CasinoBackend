import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { FaGamepad } from 'react-icons/fa';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import '../Games/Slots.css';
import './Blackjack.css';

const Blackjack = () => {
  const { balance, updateBalance } = useAuth();
  const [bet, setBet] = useState(10);
  const [gameState, setGameState] = useState('betting'); // betting, playing, ended
  const [playerHand, setPlayerHand] = useState([]);
  const [dealerHand, setDealerHand] = useState([]);
  const [result, setResult] = useState(null);

  const suits = ['♠', '♥', '♦', '♣'];
  const values = ['A', '2', '3', '4', '5', '6', '7', '8', '9', '10', 'J', 'Q', 'K'];

  const drawCard = () => {
    const suit = suits[Math.floor(Math.random() * suits.length)];
    const value = values[Math.floor(Math.random() * values.length)];
    return { suit, value };
  };

  const calculateHandValue = (hand) => {
    let value = 0;
    let aces = 0;

    hand.forEach(card => {
      if (card.value === 'A') {
        aces += 1;
        value += 11;
      } else if (['J', 'Q', 'K'].includes(card.value)) {
        value += 10;
      } else {
        value += parseInt(card.value);
      }
    });

    while (value > 21 && aces > 0) {
      value -= 10;
      aces -= 1;
    }

    return value;
  };

  const startGame = () => {
    if (balance < bet) {
      setResult({ type: 'error', message: 'Insufficient balance!' });
      return;
    }

    updateBalance(-bet);
    const newPlayerHand = [drawCard(), drawCard()];
    const newDealerHand = [drawCard(), drawCard()];
    
    setPlayerHand(newPlayerHand);
    setDealerHand(newDealerHand);
    setGameState('playing');
    setResult(null);

    // Check for natural blackjack
    if (calculateHandValue(newPlayerHand) === 21) {
      endGame(newPlayerHand, newDealerHand);
    }
  };

  const hit = () => {
    const newHand = [...playerHand, drawCard()];
    setPlayerHand(newHand);

    if (calculateHandValue(newHand) > 21) {
      endGame(newHand, dealerHand);
    }
  };

  const stand = () => {
    let newDealerHand = [...dealerHand];
    
    while (calculateHandValue(newDealerHand) < 17) {
      newDealerHand.push(drawCard());
    }
    
    setDealerHand(newDealerHand);
    endGame(playerHand, newDealerHand);
  };

  const endGame = (pHand, dHand) => {
    const playerValue = calculateHandValue(pHand);
    const dealerValue = calculateHandValue(dHand);

    setGameState('ended');

    if (playerValue > 21) {
      setResult({ type: 'lose', message: 'Bust! You lose.' });
    } else if (dealerValue > 21) {
      const winAmount = bet * 2;
      updateBalance(winAmount);
      setResult({ type: 'win', message: `Dealer busts! You win $${winAmount}!` });
    } else if (playerValue > dealerValue) {
      const winAmount = bet * 2;
      updateBalance(winAmount);
      setResult({ type: 'win', message: `You win $${winAmount}!` });
    } else if (playerValue === dealerValue) {
      updateBalance(bet);
      setResult({ type: 'push', message: 'Push! Bet returned.' });
    } else {
      setResult({ type: 'lose', message: 'Dealer wins.' });
    }
  };

  const renderCard = (card, hidden = false) => {
    if (hidden) {
      return <div className="playing-card card-back">?</div>;
    }
    return (
      <div className={`playing-card ${['♥', '♦'].includes(card.suit) ? 'red' : 'black'}`}>
        <div className="card-value">{card.value}</div>
        <div className="card-suit">{card.suit}</div>
      </div>
    );
  };

  return (
    <div className="game-container">
      <div className="game-content">
        <h1 className="game-title"><FaGamepad /> Blackjack</h1>
        
        <Card className="game-card-main">
          {gameState !== 'betting' && (
            <div className="blackjack-table">
              <div className="dealer-section">
                <h3>Dealer {gameState === 'ended' && `(${calculateHandValue(dealerHand)})`}</h3>
                <div className="card-hand">
                  {dealerHand.map((card, index) => (
                    <div key={index}>
                      {gameState === 'playing' && index === 1 
                        ? renderCard(card, true)
                        : renderCard(card)
                      }
                    </div>
                  ))}
                </div>
              </div>

              <div className="player-section">
                <h3>You ({calculateHandValue(playerHand)})</h3>
                <div className="card-hand">
                  {playerHand.map((card, index) => (
                    <div key={index}>{renderCard(card)}</div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {result && (
            <div className={`game-result ${result.type}`}>
              {result.message}
            </div>
          )}

          {gameState === 'betting' && (
            <div className="betting-controls">
              <div className="bet-selector">
                <label>Place Your Bet</label>
                <div className="bet-buttons">
                  {[10, 25, 50, 100].map(amount => (
                    <button
                      key={amount}
                      className={`bet-button ${bet === amount ? 'active' : ''}`}
                      onClick={() => setBet(amount)}
                    >
                      ${amount}
                    </button>
                  ))}
                </div>
              </div>
              <Button variant="primary" size="large" fullWidth onClick={startGame}>
                Deal Cards
              </Button>
            </div>
          )}

          {gameState === 'playing' && (
            <div className="action-buttons">
              <Button variant="primary" onClick={hit}>Hit</Button>
              <Button variant="secondary" onClick={stand}>Stand</Button>
            </div>
          )}

          {gameState === 'ended' && (
            <Button variant="primary" size="large" fullWidth onClick={() => setGameState('betting')}>
              New Game
            </Button>
          )}
        </Card>
      </div>
    </div>
  );
};

export default Blackjack;
