import React from 'react';
import { NavLink } from 'react-router-dom';
import { FaDiceD6, FaGem, FaTrophy, FaFire, FaStar } from 'react-icons/fa';
import './SlotsSidebar.css';

const SlotsSidebar = () => {
  const slotGames = [
    {
      id: 'beat-atlas',
      name: 'Beat Atlas',
      icon: FaDiceD6,
      path: '/slots/beat-atlas',
      available: true
    },
    {
      id: 'diamond-rush',
      name: 'Diamond Rush',
      icon: FaGem,
      path: '/slots/diamond-rush',
      available: false
    },
    {
      id: 'lucky-seven',
      name: 'Lucky Seven',
      icon: FaTrophy,
      path: '/slots/lucky-seven',
      available: false
    },
    {
      id: 'fire-jackpot',
      name: 'Fire Jackpot',
      icon: FaFire,
      path: '/slots/fire-jackpot',
      available: false
    },
    {
      id: 'mega-fortune',
      name: 'Mega Fortune',
      icon: FaStar,
      path: '/slots/mega-fortune',
      available: false
    }
  ];

  return (
    <aside className="slots-sidebar">
      <nav className="sidebar-nav">
        {slotGames.map(game => (
          <NavLink
            key={game.id}
            to={game.path}
            className={({ isActive }) => 
              `sidebar-slot-link ${isActive ? 'active' : ''} ${!game.available ? 'disabled' : ''}`
            }
            onClick={(e) => !game.available && e.preventDefault()}
          >
            <game.icon className="slot-icon" />
            <span className="slot-name">{game.name}</span>
            {!game.available && <span className="coming-soon-badge">Coming Soon</span>}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
};

export default SlotsSidebar;
