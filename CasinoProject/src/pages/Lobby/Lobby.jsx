import React from 'react';
import { useNavigate } from 'react-router-dom';
import { FaUsers, FaTrophy, FaGamepad, FaDiceD6, FaStar, FaDotCircle, FaCircle } from 'react-icons/fa';
import Card from '../../components/common/Card';
import './Lobby.css';

const Lobby = () => {
  const navigate = useNavigate();

  const games = [
    {
      id: 'slots',
      name: 'Slots',
      icon: FaDiceD6,
      description: 'Spin the reels for big wins',
      color: 'gold',
      path: '/slots'
    }
  ];

  return (
    <div className="lobby-container">
      <div className="lobby-hero">
        <h1 className="lobby-title">Welcome to The Silver Slayed</h1>
        <p className="lobby-subtitle">Choose your game and start winning</p>
      </div>

      <div className="games-grid">
        {games.map(game => (
          <Card 
            key={game.id}
            className={`game-card game-card-${game.color}`}
            onClick={() => navigate(game.path)}
          >
            <div className="game-icon"><game.icon /></div>
            <h2 className="game-name">{game.name}</h2>
            <p className="game-description">{game.description}</p>
            <div className="game-play-button">
              Play Now →
            </div>
          </Card>
        ))}
      </div>

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
