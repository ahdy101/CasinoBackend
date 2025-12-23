import React, { useState } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { FaDiceD6, FaGem, FaTrophy, FaFire, FaStar, FaSearch } from 'react-icons/fa';
import Card from '../../components/common/Card';
import { ATLAS } from '../../constants/images';
import './SlotsLanding.css';

const SlotsLanding = () => {
  const navigate = useNavigate();
  const { isAuthenticated, isAdmin } = useAuth();
  const [searchQuery, setSearchQuery] = useState('');

  // Redirect admin away from games
  if (isAdmin) {
    return <Navigate to="/admin" replace />;
  }

  const slotGames = [
    {
      id: 'beat-atlas',
      name: 'Beat Atlas',
      icon: FaDiceD6,
      image: ATLAS,
      description: 'Spin the reels and defeat the mighty Atlas!',
      path: '/slots/beat-atlas',
      available: true,
      jackpot: '$25,000'
    },
    {
      id: 'diamond-rush',
      name: 'Diamond Rush',
      icon: FaGem,
      description: 'Chase diamonds for massive wins!',
      path: '/slots/diamond-rush',
      available: false,
      jackpot: '$50,000'
    },
    {
      id: 'lucky-seven',
      name: 'Lucky Seven',
      icon: FaTrophy,
      description: 'Lucky sevens bring fortune and glory!',
      path: '/slots/lucky-seven',
      available: false,
      jackpot: '$30,000'
    },
    {
      id: 'fire-jackpot',
      name: 'Fire Jackpot',
      icon: FaFire,
      description: 'Hot reels, hotter wins!',
      path: '/slots/fire-jackpot',
      available: false,
      jackpot: '$40,000'
    },
    {
      id: 'mega-fortune',
      name: 'Mega Fortune',
      icon: FaStar,
      description: 'The ultimate progressive jackpot!',
      path: '/slots/mega-fortune',
      available: false,
      jackpot: '$100,000'
    }
  ];

  const filteredGames = slotGames.filter(game =>
    game.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    game.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="slots-landing-container">
      <div className="slots-hero">
        <h1 className="slots-title">Slot Games</h1>
        <p className="slots-subtitle">Spin to win big on our exciting slot machines</p>
      </div>

      <div className="search-section">
        <div className="search-bar">
          <FaSearch className="search-icon" />
          <input
            type="text"
            placeholder="Search slot games..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="search-input"
          />
        </div>
      </div>

      <div className="slots-grid">
        {filteredGames.map(game => (
          <div key={game.id} className="slot-card-wrapper">
            <Card 
              className={`slot-card ${!game.available ? 'slot-disabled' : ''}`}
              onClick={() => {
                if (!game.available) return;
                if (!isAuthenticated) {
                  navigate('/login');
                } else {
                  navigate(game.path);
                }
              }}
            >
              {!game.available && <div className="coming-soon-badge">Coming Soon</div>}
              <div className="slot-icon">
                {game.image ? (
                  <img src={game.image} alt={game.name} className="slot-game-image" />
                ) : (
                  <game.icon />
                )}
              </div>
            </Card>
            <h3 className="slot-card-title">{game.name}</h3>
          </div>
        ))}
      </div>

      {filteredGames.length === 0 && (
        <div className="no-results">
          <p>No slot games found matching "{searchQuery}"</p>
        </div>
      )}
    </div>
  );
};

export default SlotsLanding;
