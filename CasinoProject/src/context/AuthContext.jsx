import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';

const AuthContext = createContext();
const API_URL = 'http://localhost:5001/api';

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [isAdmin, setIsAdmin] = useState(false);
  const [balance, setBalance] = useState(10000); // Starting balance in chips
  const [transactions, setTransactions] = useState([]);
  const [gameHistory, setGameHistory] = useState([]);
  const [stats, setStats] = useState({
    totalGames: 0,
    totalWins: 0,
    totalLosses: 0,
    winRate: 0,
    biggestWin: 0,
    totalWagered: 0
  });

  useEffect(() => {
    // Load saved data from localStorage
    const savedUser = localStorage.getItem('user');
    const savedBalance = localStorage.getItem('balance');
    const savedTransactions = localStorage.getItem('transactions');
    const savedGameHistory = localStorage.getItem('gameHistory');
    const savedStats = localStorage.getItem('stats');

    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
    if (savedBalance) {
      setBalance(parseFloat(savedBalance));
    }
    if (savedTransactions) {
      setTransactions(JSON.parse(savedTransactions));
    }
    if (savedGameHistory) {
      setGameHistory(JSON.parse(savedGameHistory));
    }
    if (savedStats) {
      setStats(JSON.parse(savedStats));
    }
  }, []);

  // Save data to localStorage whenever it changes
  useEffect(() => {
    if (user) {
      localStorage.setItem('balance', balance.toString());
      localStorage.setItem('transactions', JSON.stringify(transactions));
      localStorage.setItem('gameHistory', JSON.stringify(gameHistory));
      localStorage.setItem('stats', JSON.stringify(stats));
    }
  }, [balance, transactions, gameHistory, stats, user]);

  const login = async (email, password) => {
    try {
      console.log('Attempting login to:', `${API_URL}/auth/login`);
      console.log('Email:', email);
      
      const response = await axios.post(`${API_URL}/auth/login`, {
        email: email,
        password: password
      }, {
        headers: {
          'X-API-KEY': 'default_tenant_api_key_12345'
        }
      });

      console.log('Login response:', response.data);
      const { token, user: userData } = response.data;
      
      const isAdminLogin = email === 'admin@casinoapi.com' || email === 'admin@silverslayed.com';
      
      const user = {
        id: userData.id,
        email: userData.email,
        name: userData.username,
        joinDate: userData.createdAt,
        role: isAdminLogin ? 'admin' : 'user',
        settings: {
          notifications: true,
          emailUpdates: true,
          soundEffects: true,
          animations: true,
          language: 'en'
        }
      };

      setUser(user);
      setBalance(userData.balance);
      setIsAdmin(isAdminLogin);
      
      localStorage.setItem('user', JSON.stringify(user));
      localStorage.setItem('token', token);
      localStorage.setItem('balance', userData.balance.toString());
      localStorage.setItem('isAdmin', isAdminLogin.toString());
      
      return { success: true };
    } catch (error) {
      console.error('Login error:', error);
      console.error('Error response:', error.response);
      console.error('Error message:', error.message);
      
      let errorMessage = 'Login failed';
      
      if (error.code === 'ERR_NETWORK') {
        errorMessage = 'Cannot connect to server. Please ensure the API is running on port 5001.';
      } else if (error.response) {
        errorMessage = error.response.data?.message || error.response.data?.Message || `Server error: ${error.response.status}`;
      } else if (error.request) {
        errorMessage = 'No response from server. Please check your connection.';
      }
      
      return { success: false, message: errorMessage };
    }
  };

  const register = async (email, password, name) => {
    try {
      const response = await axios.post(`${API_URL}/auth/register`, {
        username: name || email.split('@')[0],
        email: email,
        password: password
      }, {
        headers: {
          'X-API-KEY': 'default_tenant_api_key_12345'
        }
      });

      const userData = response.data;
      const user = {
        id: userData.id,
        email: userData.email,
        name: userData.username,
        joinDate: userData.createdAt,
        settings: {
          notifications: true,
          emailUpdates: true,
          soundEffects: true,
          animations: true,
          language: 'en'
        }
      };

      setUser(user);
      setBalance(userData.balance);
      localStorage.setItem('user', JSON.stringify(user));
      localStorage.setItem('balance', userData.balance.toString());
      
      return { success: true, message: 'Welcome bonus: 1,000 chips added!' };
    } catch (error) {
      console.error('Registration error:', error);
      const errorMessage = error.response?.data?.message || error.response?.data?.Message || 'Registration failed';
      return { success: false, message: errorMessage };
    }
  };

  const logout = () => {
    setUser(null);
    setBalance(10000);
    setTransactions([]);
    setGameHistory([]);
    setStats({
      totalGames: 0,
      totalWins: 0,
      totalLosses: 0,
      winRate: 0,
      biggestWin: 0,
      totalWagered: 0
    });
    localStorage.removeItem('user');
    localStorage.removeItem('balance');
    localStorage.removeItem('transactions');
    localStorage.removeItem('gameHistory');
    localStorage.removeItem('stats');
  };

  const updateBalance = (amount) => {
    setBalance(prev => prev + amount);
  };

  const purchaseChips = (chips, price) => {
    // Mock purchase - in real app, this would call payment API
    setBalance(prev => prev + chips);
    return true;
  };

  const addTransaction = (transaction) => {
    const newTransaction = {
      id: `TXN-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      timestamp: new Date().toISOString(),
      ...transaction
    };
    setTransactions(prev => [newTransaction, ...prev]);
  };

  const addGameResult = (game) => {
    const gameRecord = {
      timestamp: new Date().toISOString(),
      ...game
    };
    setGameHistory(prev => [gameRecord, ...prev].slice(0, 100)); // Keep last 100 games

    // Update stats
    setStats(prev => {
      const newTotalGames = prev.totalGames + 1;
      const newTotalWins = game.result === 'win' ? prev.totalWins + 1 : prev.totalWins;
      const newTotalLosses = game.result === 'loss' ? prev.totalLosses + 1 : prev.totalLosses;
      const newWinRate = newTotalGames > 0 ? Math.round((newTotalWins / newTotalGames) * 100) : 0;
      const newBiggestWin = game.result === 'win' && game.amount > prev.biggestWin ? game.amount : prev.biggestWin;
      const newTotalWagered = prev.totalWagered + Math.abs(game.amount);

      return {
        totalGames: newTotalGames,
        totalWins: newTotalWins,
        totalLosses: newTotalLosses,
        winRate: newWinRate,
        biggestWin: newBiggestWin,
        totalWagered: newTotalWagered
      };
    });
  };

  const updateUserSettings = (newSettings) => {
    setUser(prev => ({
      ...prev,
      ...newSettings,
      settings: newSettings.settings || prev.settings
    }));
    localStorage.setItem('user', JSON.stringify({
      ...user,
      ...newSettings
    }));
  };

  const changePassword = (currentPassword, newPassword) => {
    // Mock password change - replace with actual API call
    console.log('Password changed successfully');
    return true;
  };

  return (
    <AuthContext.Provider value={{ 
      user, 
      isAdmin,
      balance, 
      transactions,
      gameHistory,
      stats,
      login, 
      register, 
      logout, 
      updateBalance,
      purchaseChips,
      addTransaction,
      addGameResult,
      updateUserSettings,
      changePassword,
      isAuthenticated: !!user 
    }}>
      {children}
    </AuthContext.Provider>
  );
};
