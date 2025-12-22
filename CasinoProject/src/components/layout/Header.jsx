import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useTheme } from '../../context/ThemeContext';
import { MdLightMode, MdDarkMode, MdAccountBalanceWallet, MdHistory, MdSettings, MdLogout, MdDashboard, MdSave } from 'react-icons/md';
import { LOGO_ICON } from '../../constants/images';
import './Header.css';

const Header = () => {
  const { user, balance, logout, isAdmin } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const [showProfileMenu, setShowProfileMenu] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
    setShowProfileMenu(false);
  };

  const formatBalance = (amount) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  return (
    <header className="header">
      <div className="header-content">
        <Link to="/" className="logo">
          <img src={LOGO_ICON} alt="The Silver Slayed" className="logo-icon" />
          <div className="logo-text">
            <span className="logo-title">The Silver Slayed</span>
            <span className="logo-subtitle">Luxury Casino</span>
          </div>
        </Link>

        <nav className="nav-links">
          <Link to="/lobby" className="nav-link">Games</Link>
          <Link to="/about" className="nav-link">About</Link>
          <Link to="/rules" className="nav-link">Rules</Link>
          <Link to="/privacy" className="nav-link">Privacy</Link>
          <Link to="/contact" className="nav-link">Contact</Link>
        </nav>

        <div className="header-actions">
          {user && (
            <div className="balance-widget">
              <span className="balance-label">Balance</span>
              <span className="balance-amount">{formatBalance(balance)}</span>
            </div>
          )}

          <button className="theme-toggle" onClick={toggleTheme}>
            {theme === 'light' ? <MdDarkMode /> : <MdLightMode />}
          </button>

          {user ? (
            <div className="profile-menu-wrapper">
              <button 
                className="profile-button"
                onClick={() => setShowProfileMenu(!showProfileMenu)}
              >
                <div className="profile-avatar">
                  {user.name?.charAt(0).toUpperCase() || 'U'}
                </div>
                <span className="profile-name">{user.name}</span>
              </button>
              
              {showProfileMenu && (
                <div className="profile-dropdown">
                  <div className="profile-info">
                    <strong>{user.name}</strong>
                    <small>{user.email}</small>
                  </div>
                  <div className="profile-divider"></div>
                  {isAdmin && (
                    <>
                      <Link to="/admin" className="profile-menu-item admin"><MdDashboard /> Admin Dashboard</Link>
                      <div className="profile-divider"></div>
                    </>
                  )}
                  <Link to="/wallet" className="profile-menu-item"><MdAccountBalanceWallet /> Wallet</Link>
                  <Link to="/transactions" className="profile-menu-item"><MdHistory /> Transactions</Link>
                  <Link to="/settings" className="profile-menu-item"><MdSettings /> Settings</Link>
                  <div className="profile-divider"></div>
                  <button onClick={handleLogout} className="profile-menu-item logout">
                    <MdLogout /> Logout
                  </button>
                </div>
              )}
            </div>
          ) : (
            <Link to="/login">
              <button className="login-button">Login</button>
            </Link>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;
