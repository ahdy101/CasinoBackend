import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';
import { API_ENDPOINTS, API_BASE_URL } from '../config/api';

const AuthContext = createContext();
const API_URL = API_ENDPOINTS.AUTH;

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

  const login = async (username, password) => {
    try {
      console.log('Attempting login to:', `${API_URL}/login`);
      console.log('Username:', username);
      
      const response = await axios.post(`${API_URL}/login`, {
        username: username,
        password: password
      });

      console.log('Login response:', response.data);
      
      // Handle both response formats
      let token, userData;
      if (response.data.user) {
        // Format: { token, user: {...} }
        token = response.data.token;
        userData = response.data.user;
      } else {
        // Format: { token, username, ... } - properties at root
        token = response.data.token;
        userData = response.data;
      }
      
      const isAdminLogin = username === 'admin' || username === 'administrator';
      
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
      setBalance(userData.balance || 10000);
      setIsAdmin(isAdminLogin);
      
      localStorage.setItem('user', JSON.stringify(user));
      localStorage.setItem('token', token);
      localStorage.setItem('balance', (userData.balance || 10000).toString());
      localStorage.setItem('isAdmin', isAdminLogin.toString());
      
      return { success: true };
    } catch (error) {
      console.error('Login error:', error);
      console.error('Error response:', error.response);
      console.error('Error message:', error.message);
      
      let errorMessage = 'Login failed';
      
      if (error.code === 'ERR_NETWORK') {
        const port = API_BASE_URL.match(/:(\d+)/)?.[1] || '5001';
        errorMessage = `Cannot connect to server. Please ensure the API is running on port ${port}.`;
      } else if (error.response) {
        errorMessage = error.response.data?.message || error.response.data?.Message || `Server error: ${error.response.status}`;
      } else if (error.request) {
        errorMessage = 'No response from server. Please check your connection.';
      }
      
      return { success: false, message: errorMessage };
    }
  };

  const register = async (username, email, password, initialBalance = 10000) => {
    try {
      console.log('Sending register request:', { username, email, password, initialBalance });
      
      const response = await axios.post(`${API_URL}/register`, {
        username: username,
        email: email,
        password: password,
        initialBalance: initialBalance
      });

      console.log('Register response:', response.data);

      // Handle response - { token, user: {...} }
      const { token, user: userData } = response.data;
      
      if (!token) {
        console.error('Token is missing from response:', response.data);
        return { success: false, message: 'Invalid response from server - token missing' };
      }
      
      if (!userData || !userData.id) {
        console.error('User data is missing from response:', response.data);
        return { success: false, message: 'Invalid response from server - user data missing' };
      }
      
      const user = {
        id: userData.id,
        email: userData.email,
        name: userData.username,
        joinDate: userData.createdAt,
        role: 'user',
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
      setIsAdmin(false); // New users are not admins
      localStorage.setItem('user', JSON.stringify(user));
      localStorage.setItem('token', token);
      localStorage.setItem('balance', userData.balance.toString());
      localStorage.setItem('isAdmin', 'false');
      
      return { success: true, message: `Welcome! You received ${userData.balance.toLocaleString()} chips!` };
    } catch (error) {
      console.error('Registration error:', error);
      console.error('Error response:', error.response?.data);
      console.error('Validation errors:', error.response?.data?.errors);
      
      // Handle validation errors
      if (error.response?.data?.errors) {
        const validationErrors = error.response.data.errors;
        const errorMessages = Object.entries(validationErrors)
          .map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
          .join('; ');
        return { success: false, message: errorMessages };
      }
      
      const errorMessage = error.response?.data?.message || error.response?.data?.Message || error.response?.data?.title || 'Registration failed';
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
