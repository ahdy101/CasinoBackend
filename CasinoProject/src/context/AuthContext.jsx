import React, { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext();

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

  const login = (email, password) => {
    // Mock login - replace with actual API call later
    // Check if admin login
    const isAdminLogin = email === 'admin@silverslayed.com';
    
    const mockUser = {
      id: '1',
      email: email,
      name: email.split('@')[0],
      joinDate: new Date().toISOString(),
      role: isAdminLogin ? 'admin' : 'user',
      settings: {
        notifications: true,
        emailUpdates: true,
        soundEffects: true,
        animations: true,
        language: 'en'
      }
    };
    setUser(mockUser);
    setIsAdmin(isAdminLogin);
    localStorage.setItem('user', JSON.stringify(mockUser));
    localStorage.setItem('isAdmin', isAdminLogin.toString());
    return { success: true };
  };

  const register = (email, password, name) => {
    // Mock registration - replace with actual API call later
    const mockUser = {
      id: Date.now().toString(),
      email: email,
      name: name || email.split('@')[0],
      joinDate: new Date().toISOString(),
      settings: {
        notifications: true,
        emailUpdates: true,
        soundEffects: true,
        animations: true,
        language: 'en'
      }
    };
    setUser(mockUser);
    setBalance(15000); // Welcome bonus
    localStorage.setItem('user', JSON.stringify(mockUser));
    return { success: true, message: 'Welcome bonus: 15,000 chips added!' };
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
