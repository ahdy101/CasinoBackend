import React from 'react';
import { NavLink } from 'react-router-dom';
import { FaDice, FaChess, FaDiceD6, FaGamepad, FaTrophy } from 'react-icons/fa';
import './GameTypeSidebar.css';

const GameTypeSidebar = () => {
  return (
    <aside className="game-type-sidebar">
      <nav className="game-type-nav">
        <NavLink 
          to="/slots" 
          className={({ isActive }) => isActive ? 'game-type-link active' : 'game-type-link'}
        >
          <FaDiceD6 className="game-type-icon" />
          <span className="game-type-name">Slots</span>
        </NavLink>
        
        <div className="game-type-link disabled">
          <FaGamepad className="game-type-icon" />
          <span className="game-type-name">Blackjack</span>
          <span className="coming-soon-tag">Soon</span>
        </div>

        <div className="game-type-link disabled">
          <FaDice className="game-type-icon" />
          <span className="game-type-name">Dice</span>
          <span className="coming-soon-tag">Soon</span>
        </div>

        <div className="game-type-link disabled">
          <FaChess className="game-type-icon" />
          <span className="game-type-name">Roulette</span>
          <span className="coming-soon-tag">Soon</span>
        </div>

        <div className="game-type-link disabled">
          <FaTrophy className="game-type-icon" />
          <span className="game-type-name">Poker</span>
          <span className="coming-soon-tag">Soon</span>
        </div>
      </nav>
    </aside>
  );
};

export default GameTypeSidebar;
