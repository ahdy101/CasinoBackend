import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useGameState } from '../../context/GameStateContext';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import { MdDelete, MdPlayArrow, MdBarChart } from 'react-icons/md';
import './SavedGames.css';

const SavedGames = () => {
  const navigate = useNavigate();
  const { getSavedGames, deleteGame, getAllStats } = useGameState();
  
  const savedGames = getSavedGames();
  const allStats = getAllStats();

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  const getGameName = (gameType) => {
    const names = {
      blackjack: 'Blackjack',
      poker: 'Poker',
      slots: 'Slots',
      roulette: 'Roulette'
    };
    return names[gameType] || gameType;
  };

  const handleResume = (gameType) => {
    navigate(`/${gameType}`);
  };

  const handleDelete = (gameType, e) => {
    e.stopPropagation();
    if (window.confirm(`Delete saved ${getGameName(gameType)} game?`)) {
      deleteGame(gameType);
    }
  };

  const calculateWinRate = (stats) => {
    const total = stats.wins + stats.losses + (stats.pushes || 0);
    return total > 0 ? ((stats.wins / total) * 100).toFixed(1) : 0;
  };

  const calculateProfit = (stats) => {
    return stats.totalWon - stats.totalWagered;
  };

  return (
    <div className="saved-games-container">
      <div className="saved-games-header">
        <h1 className="saved-games-title">Saved Games & Statistics</h1>
        <p className="saved-games-subtitle">Resume your games or view your performance</p>
      </div>

      {/* Saved Games Section */}
      <section className="saved-games-section">
        <h2><MdPlayArrow /> Saved Games</h2>
        
        {savedGames.length === 0 ? (
          <Card className="empty-state">
            <p>No saved games. Start playing and pause to save your progress!</p>
            <Button variant="primary" onClick={() => navigate('/lobby')}>
              Go to Lobby
            </Button>
          </Card>
        ) : (
          <div className="saved-games-grid">
            {savedGames.map(game => (
              <Card key={game.gameType} className="saved-game-card">
                <div className="saved-game-header">
                  <h3>{getGameName(game.gameType)}</h3>
                  <button 
                    className="delete-btn"
                    onClick={(e) => handleDelete(game.gameType, e)}
                    title="Delete saved game"
                  >
                    <MdDelete />
                  </button>
                </div>
                
                <div className="saved-game-info">
                  <div className="info-row">
                    <span className="label">Saved:</span>
                    <span className="value">{formatDate(game.savedAt)}</span>
                  </div>
                  {game.bet && (
                    <div className="info-row">
                      <span className="label">Current Bet:</span>
                      <span className="value bet">{formatCurrency(game.bet)}</span>
                    </div>
                  )}
                  {game.gameState && (
                    <div className="info-row">
                      <span className="label">Status:</span>
                      <span className="value status">{game.gameState}</span>
                    </div>
                  )}
                </div>

                <Button 
                  variant="primary" 
                  fullWidth 
                  onClick={() => handleResume(game.gameType)}
                >
                  Resume Game
                </Button>
              </Card>
            ))}
          </div>
        )}
      </section>

      {/* Statistics Section */}
      <section className="stats-section">
        <h2><MdBarChart /> Game Statistics</h2>
        
        <div className="stats-grid">
          {Object.entries(allStats).map(([gameType, stats]) => {
            const totalGames = stats.wins + stats.losses + (stats.pushes || 0);
            const winRate = calculateWinRate(stats);
            const profit = calculateProfit(stats);

            return (
              <Card key={gameType} className="stats-card">
                <h3>{getGameName(gameType)}</h3>
                
                <div className="stats-content">
                  <div className="stat-row">
                    <span className="stat-label">Total Games:</span>
                    <span className="stat-value">{totalGames}</span>
                  </div>
                  
                  <div className="stat-row">
                    <span className="stat-label">Wins:</span>
                    <span className="stat-value win">{stats.wins}</span>
                  </div>
                  
                  <div className="stat-row">
                    <span className="stat-label">Losses:</span>
                    <span className="stat-value loss">{stats.losses}</span>
                  </div>
                  
                  {stats.pushes > 0 && (
                    <div className="stat-row">
                      <span className="stat-label">Pushes:</span>
                      <span className="stat-value">{stats.pushes}</span>
                    </div>
                  )}
                  
                  <div className="stat-row highlight">
                    <span className="stat-label">Win Rate:</span>
                    <span className="stat-value">{winRate}%</span>
                  </div>
                  
                  <div className="stat-row">
                    <span className="stat-label">Total Wagered:</span>
                    <span className="stat-value">{formatCurrency(stats.totalWagered)}</span>
                  </div>
                  
                  <div className="stat-row">
                    <span className="stat-label">Total Won:</span>
                    <span className="stat-value">{formatCurrency(stats.totalWon)}</span>
                  </div>
                  
                  <div className={`stat-row highlight ${profit >= 0 ? 'profit' : 'loss-profit'}`}>
                    <span className="stat-label">Net Profit:</span>
                    <span className="stat-value">{formatCurrency(profit)}</span>
                  </div>
                </div>
              </Card>
            );
          })}
        </div>
      </section>
    </div>
  );
};

export default SavedGames;
