import React, { useState, useEffect } from 'react';
import Card from '../../components/common/Card';
import { MdPeople, MdAttachMoney, MdTrendingUp, MdGames } from 'react-icons/md';
import { FaUsers, FaChartLine, FaCoins } from 'react-icons/fa';
import './AdminDashboard.css';

const AdminDashboard = () => {
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      const response = await fetch('http://localhost:5000/api/admin/dashboard');
      if (!response.ok) throw new Error('Failed to fetch dashboard data');
      const data = await response.json();
      setDashboardData(data);
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  const formatNumber = (num) => {
    return new Intl.NumberFormat('en-US').format(num);
  };

  const formatDate = (date) => {
    return new Date(date).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return (
      <div className="admin-dashboard">
        <div className="loading">Loading dashboard data...</div>
      </div>
    );
  }

  if (!dashboardData) {
    return (
      <div className="admin-dashboard">
        <div className="error">Failed to load dashboard data</div>
      </div>
    );
  }

  const { userStats, transactionStats, gameStats } = dashboardData;

  return (
    <div className="admin-dashboard">
      <div className="dashboard-header">
        <h1 className="dashboard-title">Admin Dashboard</h1>
        <p className="dashboard-subtitle">Monitor and manage your casino platform</p>
      </div>

      {/* Overview Cards */}
      <div className="stats-grid">
        <Card className="stat-card">
          <div className="stat-icon users">
            <MdPeople />
          </div>
          <div className="stat-content">
            <h3 className="stat-value">{formatNumber(userStats.totalUsers)}</h3>
            <p className="stat-label">Total Users</p>
            <p className="stat-detail">+{userStats.newSignupsToday} today</p>
          </div>
        </Card>

        <Card className="stat-card">
          <div className="stat-icon active">
            <FaUsers />
          </div>
          <div className="stat-content">
            <h3 className="stat-value">{formatNumber(userStats.activeUsers)}</h3>
            <p className="stat-label">Active Users</p>
            <p className="stat-detail">Last 7 days</p>
          </div>
        </Card>

        <Card className="stat-card">
          <div className="stat-icon transactions">
            <MdAttachMoney />
          </div>
          <div className="stat-content">
            <h3 className="stat-value">{formatCurrency(transactionStats.totalVolume)}</h3>
            <p className="stat-label">Total Volume</p>
            <p className="stat-detail">{formatCurrency(transactionStats.todayVolume)} today</p>
          </div>
        </Card>

        <Card className="stat-card">
          <div className="stat-icon games">
            <MdGames />
          </div>
          <div className="stat-content">
            <h3 className="stat-value">{formatNumber(gameStats.totalGamesPlayed)}</h3>
            <p className="stat-label">Games Played</p>
            <p className="stat-detail">+{gameStats.gamesToday} today</p>
          </div>
        </Card>
      </div>

      {/* Tabs */}
      <div className="dashboard-tabs">
        <button 
          className={`tab-button ${activeTab === 'overview' ? 'active' : ''}`}
          onClick={() => setActiveTab('overview')}
        >
          Overview
        </button>
        <button 
          className={`tab-button ${activeTab === 'users' ? 'active' : ''}`}
          onClick={() => setActiveTab('users')}
        >
          Users
        </button>
        <button 
          className={`tab-button ${activeTab === 'transactions' ? 'active' : ''}`}
          onClick={() => setActiveTab('transactions')}
        >
          Transactions
        </button>
        <button 
          className={`tab-button ${activeTab === 'games' ? 'active' : ''}`}
          onClick={() => setActiveTab('games')}
        >
          Games
        </button>
      </div>

      {/* Tab Content */}
      <div className="tab-content">
        {activeTab === 'overview' && (
          <div className="overview-section">
            <div className="overview-grid">
              <Card className="overview-card">
                <h3>User Growth</h3>
                <div className="growth-stats">
                  <div className="growth-item">
                    <span className="growth-label">Today</span>
                    <span className="growth-value">{userStats.newSignupsToday}</span>
                  </div>
                  <div className="growth-item">
                    <span className="growth-label">This Week</span>
                    <span className="growth-value">{userStats.newSignupsThisWeek}</span>
                  </div>
                  <div className="growth-item">
                    <span className="growth-label">This Month</span>
                    <span className="growth-value">{userStats.newSignupsThisMonth}</span>
                  </div>
                </div>
              </Card>

              <Card className="overview-card">
                <h3>Transaction Volume</h3>
                <div className="growth-stats">
                  <div className="growth-item">
                    <span className="growth-label">Today</span>
                    <span className="growth-value">{formatCurrency(transactionStats.todayVolume)}</span>
                  </div>
                  <div className="growth-item">
                    <span className="growth-label">This Week</span>
                    <span className="growth-value">{formatCurrency(transactionStats.weekVolume)}</span>
                  </div>
                  <div className="growth-item">
                    <span className="growth-label">This Month</span>
                    <span className="growth-value">{formatCurrency(transactionStats.monthVolume)}</span>
                  </div>
                </div>
              </Card>

              <Card className="overview-card">
                <h3>House Performance</h3>
                <div className="growth-stats">
                  <div className="growth-item">
                    <span className="growth-label">Total Wagered</span>
                    <span className="growth-value">{formatCurrency(gameStats.totalWagered)}</span>
                  </div>
                  <div className="growth-item">
                    <span className="growth-label">Total Won</span>
                    <span className="growth-value">{formatCurrency(gameStats.totalWon)}</span>
                  </div>
                  <div className="growth-item">
                    <span className="growth-label">House Edge</span>
                    <span className="growth-value">{gameStats.houseEdge.toFixed(2)}%</span>
                  </div>
                </div>
              </Card>
            </div>
          </div>
        )}

        {activeTab === 'users' && (
          <Card className="data-table-card">
            <h3>Recent Users</h3>
            <div className="table-container">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Username</th>
                    <th>Email</th>
                    <th>Balance</th>
                    <th>Joined</th>
                    <th>Last Activity</th>
                  </tr>
                </thead>
                <tbody>
                  {userStats.recentUsers.map(user => (
                    <tr key={user.userId}>
                      <td>{user.username}</td>
                      <td>{user.email}</td>
                      <td>{formatCurrency(user.balance)}</td>
                      <td>{formatDate(user.createdAt)}</td>
                      <td>{user.lastActivityAt ? formatDate(user.lastActivityAt) : 'No activity'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
        )}

        {activeTab === 'transactions' && (
          <div className="transactions-section">
            <div className="transaction-stats-grid">
              <Card className="transaction-stat">
                <h4>Total Transactions</h4>
                <p className="stat-big">{formatNumber(transactionStats.totalTransactions)}</p>
                <p className="stat-small">+{transactionStats.todayTransactions} today</p>
              </Card>
              <Card className="transaction-stat">
                <h4>Average Transaction</h4>
                <p className="stat-big">{formatCurrency(transactionStats.averageTransactionAmount)}</p>
              </Card>
            </div>

            <Card className="data-table-card">
              <h3>Recent Transactions</h3>
              <div className="table-container">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Username</th>
                      <th>Game</th>
                      <th>Bet Amount</th>
                      <th>Win Amount</th>
                      <th>Date</th>
                    </tr>
                  </thead>
                  <tbody>
                    {transactionStats.recentTransactions.map(tx => (
                      <tr key={tx.betId}>
                        <td>#{tx.betId}</td>
                        <td>{tx.username}</td>
                        <td>{tx.gameType}</td>
                        <td>{formatCurrency(tx.amount)}</td>
                        <td className={tx.winAmount > 0 ? 'win' : 'loss'}>
                          {tx.winAmount ? formatCurrency(tx.winAmount) : '-'}
                        </td>
                        <td>{formatDate(tx.createdAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          </div>
        )}

        {activeTab === 'games' && (
          <div className="games-section">
            <div className="game-stats-grid">
              <Card className="game-stat">
                <h4>Games This Week</h4>
                <p className="stat-big">{formatNumber(gameStats.gamesThisWeek)}</p>
              </Card>
              <Card className="game-stat">
                <h4>Games This Month</h4>
                <p className="stat-big">{formatNumber(gameStats.gamesThisMonth)}</p>
              </Card>
            </div>

            <Card className="data-table-card">
              <h3>Game Type Statistics</h3>
              <div className="table-container">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Game Type</th>
                      <th>Games Played</th>
                      <th>Total Wagered</th>
                      <th>Total Won</th>
                      <th>Win Rate</th>
                    </tr>
                  </thead>
                  <tbody>
                    {gameStats.gameTypeStatistics.map(game => (
                      <tr key={game.gameType}>
                        <td><strong>{game.gameType}</strong></td>
                        <td>{formatNumber(game.gamesPlayed)}</td>
                        <td>{formatCurrency(game.totalWagered)}</td>
                        <td>{formatCurrency(game.totalWon)}</td>
                        <td>{game.winRate.toFixed(2)}%</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminDashboard;
