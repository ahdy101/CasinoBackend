import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { FaStar } from 'react-icons/fa';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import '../Games/Blackjack.css';
import './Poker.css';

const Poker = () => {
  const { balance, updateBalance } = useAuth();
  const [bet, setBet] = useState(10);
  const [gameState, setGameState] = useState('betting'); // betting, flop, turn, river, ended
  const [playerHand, setPlayerHand] = useState([]);
  const [communityCards, setCommunityCards] = useState([]);
  const [pot, setPot] = useState(0);
  const [result, setResult] = useState(null);

  const suits = ['♠', '♥', '♦', '♣'];
  const values = ['2', '3', '4', '5', '6', '7', '8', '9', '10', 'J', 'Q', 'K', 'A'];

  const drawCard = () => {
    const suit = suits[Math.floor(Math.random() * suits.length)];
    const value = values[Math.floor(Math.random() * values.length)];
    return { suit, value };
  };

  const startGame = () => {
    if (balance < bet) {
      setResult({ type: 'error', message: 'Insufficient balance!' });
      return;
    }

    updateBalance(-bet);
    setPot(bet * 2); // Player + dealer ante
    setPlayerHand([drawCard(), drawCard()]);
    setCommunityCards([]);
    setGameState('flop');
    setResult(null);
  };

  const handleFlop = () => {
    setCommunityCards([drawCard(), drawCard(), drawCard()]);
    setGameState('turn');
  };

  const handleTurn = () => {
    setCommunityCards(prev => [...prev, drawCard()]);
    setGameState('river');
  };

  const handleRiver = () => {
    setCommunityCards(prev => [...prev, drawCard()]);
    setGameState('ended');
    determineWinner();
  };

  const placeBet = (amount) => {
    if (balance < amount) {
      setResult({ type: 'error', message: 'Insufficient balance!' });
      return;
    }
    updateBalance(-amount);
    setPot(prev => prev + amount);
  };

  const check = () => {
    if (gameState === 'flop') handleFlop();
    else if (gameState === 'turn') handleTurn();
    else if (gameState === 'river') handleRiver();
  };

  const raiseBet = () => {
    placeBet(bet);
    check();
  };

  const fold = () => {
    setResult({ type: 'lose', message: 'You folded. Dealer wins.' });
    setGameState('ended');
    setPot(0);
  };

  const determineWinner = () => {
    // Simplified winning logic
    const playerValue = values.indexOf(playerHand[0].value) + values.indexOf(playerHand[1].value);
    const randomFactor = Math.random();

    if (randomFactor > 0.5) {
      updateBalance(pot);
      setResult({ type: 'win', message: `You win $${pot}!` });
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
        <h1 className="game-title"><FaStar /> Texas Hold'em Poker</h1>
        
        <Card className="game-card-main">
          {gameState !== 'betting' && (
            <div className="poker-table">
              <div className="community-section">
                <h3>Community Cards</h3>
                <div className="card-hand">
                  {communityCards.map((card, index) => (
                    <div key={index}>{renderCard(card)}</div>
                  ))}
                  {communityCards.length < 5 && Array(5 - communityCards.length).fill(null).map((_, i) => (
                    <div key={`empty-${i}`} className="card-placeholder">?</div>
                  ))}
                </div>
              </div>

              <div className="pot-display">
                <span className="pot-label">Pot</span>
                <span className="pot-amount">${pot}</span>
              </div>

              <div className="player-section">
                <h3>Your Hand</h3>
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
                <label>Ante Bet</label>
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

          {gameState !== 'betting' && gameState !== 'ended' && (
            <div className="poker-actions">
              <Button variant="danger" onClick={fold}>Fold</Button>
              <Button variant="outline" onClick={check}>Check</Button>
              <Button variant="primary" onClick={raiseBet}>
                Raise ${bet}
              </Button>
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

export default Poker;
