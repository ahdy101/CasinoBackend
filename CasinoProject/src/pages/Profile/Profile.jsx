import React from 'react';
import { useAuth } from '../../context/AuthContext';
import { MdAccountBalanceWallet, MdBarChart, MdHistory, MdEmojiEvents } from 'react-icons/md';
import { FaGamepad, FaStar, FaDiamond, FaFire } from 'react-icons/fa6';
import Card from '../../components/common/Card';
import './Profile.css';

const Profile = () => {
  const { user, balance, gameHistory, stats } = useAuth();

  const formatBalance = (amount) => {
    return new Intl.NumberFormat('en-US', {
      style: 'decimal',
      minimumFractionDigits: 0
    }).format(amount);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="profile-container">
      <div className="profile-header">
        <div className="profile-avatar-large">
          {user?.name?.charAt(0).toUpperCase() || 'U'}
        </div>
        <div className="profile-info-header">
          <h1 className="profile-name">{user?.name}</h1>
          <p className="profile-email">{user?.email}</p>
          <p className="profile-joined">Member since {formatDate(user?.joinDate)}</p>
        </div>
      </div>

      <div className="profile-grid">
        {/* Wallet Overview */}
        <Card className="profile-card wallet-overview">
          <h2 className="card-title"><MdAccountBalanceWallet /> Wallet Overview</h2>
          <div className="wallet-balance">
            <div className="balance-main">
              <span className="balance-label">In-Game Chips</span>
              <span className="balance-value">{formatBalance(balance)}</span>
            </div>
            <p className="balance-note">
              Your in-game currency balance. Purchase chips to play games.
            </p>
          </div>
        </Card>

        {/* Game Statistics */}
        <Card className="profile-card stats-card">
          <h2 className="card-title"><MdBarChart /> Statistics</h2>
          <div className="stats-grid">
            <div className="stat-item">
              <span className="stat-label">Total Games</span>
              <span className="stat-value">{stats?.totalGames || 0}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Total Wins</span>
              <span className="stat-value success">{stats?.totalWins || 0}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Total Losses</span>
              <span className="stat-value danger">{stats?.totalLosses || 0}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Win Rate</span>
              <span className="stat-value">{stats?.winRate || 0}%</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Biggest Win</span>
              <span className="stat-value success">+{formatBalance(stats?.biggestWin || 0)}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Total Wagered</span>
              <span className="stat-value">{formatBalance(stats?.totalWagered || 0)}</span>
            </div>
          </div>
        </Card>

        {/* Recent Game History */}
        <Card className="profile-card history-card">
          <h2 className="card-title"><MdHistory /> Recent Game History</h2>
          <div className="history-list">
            {gameHistory && gameHistory.length > 0 ? (
              gameHistory.slice(0, 10).map((game, index) => (
                <div key={index} className={`history-item ${game.result}`}>
                  <div className="history-game">
                    <span className="game-icon">{game.icon}</span>
                    <div>
                      <span className="game-name">{game.gameName}</span>
                      <span className="game-time">{formatDate(game.timestamp)}</span>
                    </div>
                  </div>
                  <div className={`history-amount ${game.result}`}>
                    {game.result === 'win' ? '+' : '-'}
                    {formatBalance(Math.abs(game.amount))}
                  </div>
                </div>
              ))
            ) : (
              <div className="no-history">
                <p>No game history yet. Start playing to see your results here!</p>
              </div>
            )}
          </div>
        </Card>

        {/* Achievements */}
        <Card className="profile-card achievements-card">
          <h2 className="card-title"><MdEmojiEvents /> Achievements</h2>
          <div className="achievements-grid">
            <div className={`achievement-item ${stats?.totalGames >= 10 ? 'unlocked' : 'locked'}`}>
              <span className="achievement-icon"><FaGamepad /></span>
              <span className="achievement-name">Beginner</span>
              <span className="achievement-desc">Play 10 games</span>
            </div>
            <div className={`achievement-item ${stats?.totalWins >= 5 ? 'unlocked' : 'locked'}`}>
              <span className="achievement-icon"><FaStar /></span>
              <span className="achievement-name">Winner</span>
              <span className="achievement-desc">Win 5 games</span>
            </div>
            <div className={`achievement-item ${stats?.biggestWin >= 1000 ? 'unlocked' : 'locked'}`}>
              <span className="achievement-icon"><FaDiamond /></span>
              <span className="achievement-name">High Roller</span>
              <span className="achievement-desc">Win 1000+ chips</span>
            </div>
            <div className={`achievement-item ${stats?.totalGames >= 100 ? 'unlocked' : 'locked'}`}>
              <span className="achievement-icon"><FaFire /></span>
              <span className="achievement-name">Veteran</span>
              <span className="achievement-desc">Play 100 games</span>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
};

export default Profile;
