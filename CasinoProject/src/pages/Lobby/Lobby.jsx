import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaUsers, FaTrophy, FaGamepad, FaDiceD6, FaStar, FaHeart, FaDice, FaCoins, FaCircle, FaSearch } from 'react-icons/fa';
import Card from '../../components/common/Card';
import './Lobby.css';

const Lobby = () => {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');

  const allGames = [
    {
      id: 'slots',
      name: 'Slots',
      icon: FaDiceD6,
      description: 'Spin the reels and win big with Atlas!',
      color: 'gold',
      path: '/slots',
      available: true,
      players: 234
    },
    {
      id: 'blackjack',
      name: 'Blackjack',
      icon: FaHeart,
      description: 'Beat the dealer and get 21!',
      color: 'silver',
      path: '/blackjack',
      available: false,
      players: 0
    },
    {
      id: 'dice',
      name: 'Dice',
      icon: FaDice,
      description: 'Roll the dice for instant wins!',
      color: 'gold',
      path: '/dice',
      available: false,
      players: 0
    },
    {
      id: 'roulette',
      name: 'Roulette',
      icon: FaCircle,
      description: 'Bet on red or black, place your bets!',
      color: 'silver',
      path: '/roulette',
      available: false,
      players: 0
    },
    {
      id: 'poker',
      name: 'Poker',
      icon: FaCoins,
      description: 'Test your skills in Texas Hold\'em!',
      color: 'gold',
      path: '/poker',
      available: false,
      players: 0
    }
  ];

  const filteredGames = allGames.filter(game =>
    game.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    game.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="lobby-container">
      <div className="lobby-hero">
        <h1 className="lobby-title">Welcome to The Silver Slayed</h1>
        <p className="lobby-subtitle">Choose your game and start winning</p>
      </div>

      <div className="search-section">
        <div className="search-bar">
          <FaSearch className="search-icon" />
          <input
            type="text"
            placeholder="Search games..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="search-input"
          />
        </div>
      </div>

      <div className="games-grid">
        {filteredGames.map(game => (
          <Card 
            key={game.id}
            className={`game-card game-card-${game.color} ${!game.available ? 'game-disabled' : ''}`}
            onClick={() => game.available && navigate(game.path)}
          >
            {!game.available && <div className="coming-soon-badge">Coming Soon</div>}
            <div className="game-icon"><game.icon /></div>
            <h2 className="game-name">{game.name}</h2>
            <p className="game-description">{game.description}</p>
            {game.available && (
              <div className="game-players">
                <FaUsers className="players-icon" />
                <span>{game.players} playing now</span>
              </div>
            )}
            <div className={`game-play-button ${!game.available ? 'disabled' : ''}`}>
              {game.available ? 'Play Now →' : 'Coming Soon'}
            </div>
          </Card>
        ))}
      </div>

      {filteredGames.length === 0 && (
        <div className="no-results">
          <p>No games found matching "{searchQuery}"</p>
        </div>
      )}

      <div className="lobby-stats">
        <Card className="stat-card">
          <div className="stat-icon"><FaUsers /></div>
          <div className="stat-value">1,234</div>
          <div className="stat-label">Players Online</div>
        </Card>
        <Card className="stat-card">
          <div className="stat-icon"><FaTrophy /></div>
          <div className="stat-value">$125K</div>
          <div className="stat-label">Won Today</div>
        </Card>
        <Card className="stat-card">
          <div className="stat-icon"><FaGamepad /></div>
          <div className="stat-value">5,678</div>
          <div className="stat-label">Games Played</div>
        </Card>
      </div>
    </div>
  );
};

export default Lobby;
