import React from 'react';
import { NavLink } from 'react-router-dom';
import { FaDiceD6, FaDice, FaCoins, FaCircle, FaHeart } from 'react-icons/fa';
import './GameSidebar.css';

const GameSidebar = () => {
  const games = [
    {
      id: 'slots',
      name: 'Slots',
      icon: FaDiceD6,
      path: '/slots',
      available: true
    },
    {
      id: 'blackjack',
      name: 'Blackjack',
      icon: FaHeart,
      path: '/blackjack',
      available: false
    },
    {
      id: 'dice',
      name: 'Dice',
      icon: FaDice,
      path: '/dice',
      available: false
    },
    {
      id: 'roulette',
      name: 'Roulette',
      icon: FaCircle,
      path: '/roulette',
      available: false
    },
    {
      id: 'poker',
      name: 'Poker',
      icon: FaCoins,
      path: '/poker',
      available: false
    }
  ];

  return (
    <aside className="game-sidebar">
      <div className="sidebar-header">
        <h3>Casino Games</h3>
      </div>
      <nav className="sidebar-nav">
        {games.map(game => (
          <NavLink
            key={game.id}
            to={game.path}
            className={({ isActive }) => 
              `sidebar-game-link ${isActive ? 'active' : ''} ${!game.available ? 'disabled' : ''}`
            }
            onClick={(e) => !game.available && e.preventDefault()}
          >
            <game.icon className="game-icon" />
            <span className="game-name">{game.name}</span>
            {!game.available && <span className="coming-soon">Coming Soon</span>}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
};

export default GameSidebar;
