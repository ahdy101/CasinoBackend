import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';

const GameStateContext = createContext();
import { API_ENDPOINTS } from '../config/api';

const API_URL = API_ENDPOINTS.GAMESTATE;

export const useGameState = () => {
  const context = useContext(GameStateContext);
  if (!context) {
    throw new Error('useGameState must be used within GameStateProvider');
  }
  return context;
};

export const GameStateProvider = ({ children }) => {
  const [savedGames, setSavedGames] = useState({});
  const [gameStats, setGameStats] = useState({
    blackjack: { wins: 0, losses: 0, pushes: 0, totalWagered: 0, totalWon: 0 },
    poker: { wins: 0, losses: 0, pushes: 0, totalWagered: 0, totalWon: 0 },
    slots: { wins: 0, losses: 0, totalWagered: 0, totalWon: 0 },
    roulette: { wins: 0, losses: 0, totalWagered: 0, totalWon: 0 }
  });

  // Load saved data on mount
  useEffect(() => {
    const savedGamesData = localStorage.getItem('savedGames');
    const gameStatsData = localStorage.getItem('gameStats');

    if (savedGamesData) {
      try {
        setSavedGames(JSON.parse(savedGamesData));
      } catch (error) {
        console.error('Error loading saved games:', error);
      }
    }

    if (gameStatsData) {
      try {
        setGameStats(JSON.parse(gameStatsData));
      } catch (error) {
        console.error('Error loading game stats:', error);
      }
    }
  }, []);

  // Auto-save game state to API
  const saveGame = async (gameType, gameState) => {
    try {
      const token = localStorage.getItem('token');
      await axios.post(`${API_URL}/save`, {
        gameType,
        state: JSON.stringify(gameState)
      }, {
        headers: { Authorization: `Bearer ${token}` }
      });

      setSavedGames(prev => ({
        ...prev,
        [gameType]: {
          ...gameState,
          savedAt: new Date().toISOString(),
          gameType
        }
      }));
    } catch (error) {
      console.error('Error saving game:', error);
      // Fallback to localStorage
      const updated = {
        ...savedGames,
        [gameType]: { ...gameState, savedAt: new Date().toISOString(), gameType }
      };
      setSavedGames(updated);
      localStorage.setItem('savedGames', JSON.stringify(updated));
    }
  };

  const loadGame = async (gameType) => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_URL}/load/${gameType}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      
      const gameState = JSON.parse(response.data.state);
      return gameState;
    } catch (error) {
      console.error('Error loading game:', error);
      // Fallback to localStorage
      return savedGames[gameType] || null;
    }
  };

  const deleteGame = async (gameType) => {
    try {
      const token = localStorage.getItem('token');
      await axios.delete(`${API_URL}/${gameType}`, {
        headers: { Authorization: `Bearer ${token}` }
      });

      setSavedGames(prev => {
        const newSaved = { ...prev };
        delete newSaved[gameType];
        return newSaved;
      });
    } catch (error) {
      console.error('Error deleting game:', error);
      // Still delete locally
      setSavedGames(prev => {
        const newSaved = { ...prev };
        delete newSaved[gameType];
        return newSaved;
      });
      localStorage.setItem('savedGames', JSON.stringify(savedGames));
    }
  };

  const hasSavedGame = async (gameType) => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_URL}/has/${gameType}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      return response.data.hasSavedGame;
    } catch (error) {
      console.error('Error checking saved game:', error);
      return !!savedGames[gameType];
    }
  };

  const updateGameStats = async (gameType, result, wagered, won) => {
    try {
      const token = localStorage.getItem('token');
      await axios.post(`${API_URL}/stats`, {
        gameType,
        result,
        wagered,
        won
      }, {
        headers: { Authorization: `Bearer ${token}` }
      });

      // Update local state
      setGameStats(prev => {
        const currentStats = prev[gameType] || { wins: 0, losses: 0, pushes: 0, totalWagered: 0, totalWon: 0 };
        
        return {
          ...prev,
          [gameType]: {
            wins: result === 'win' ? currentStats.wins + 1 : currentStats.wins,
            losses: result === 'loss' ? currentStats.losses + 1 : currentStats.losses,
            pushes: result === 'push' ? currentStats.pushes + 1 : currentStats.pushes,
            totalWagered: currentStats.totalWagered + wagered,
            totalWon: currentStats.totalWon + won
          }
        };
      });
    } catch (error) {
      console.error('Error updating game stats:', error);
      // Fallback to localStorage
      setGameStats(prev => {
        const currentStats = prev[gameType] || { wins: 0, losses: 0, pushes: 0, totalWagered: 0, totalWon: 0 };
        const updated = {
          ...prev,
          [gameType]: {
            wins: result === 'win' ? currentStats.wins + 1 : currentStats.wins,
            losses: result === 'loss' ? currentStats.losses + 1 : currentStats.losses,
            pushes: result === 'push' ? currentStats.pushes + 1 : currentStats.pushes,
            totalWagered: currentStats.totalWagered + wagered,
            totalWon: currentStats.totalWon + won
          }
        };
        localStorage.setItem('gameStats', JSON.stringify(updated));
        return updated;
      });
    }
  };

  const getGameStats = async (gameType) => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_URL}/stats/${gameType}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      return response.data;
    } catch (error) {
      console.error('Error getting game stats:', error);
      return gameStats[gameType] || { wins: 0, losses: 0, pushes: 0, totalWagered: 0, totalWon: 0 };
    }
  };

  const getAllStats = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_URL}/stats`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      
      // Convert array to object format
      const statsObject = {};
      response.data.stats.forEach(stat => {
        statsObject[stat.gameType] = {
          wins: stat.wins,
          losses: stat.losses,
          pushes: stat.pushes,
          totalWagered: stat.totalWagered,
          totalWon: stat.totalWon,
          winRate: stat.winRate,
          profit: stat.profit
        };
      });
      
      setGameStats(statsObject);
      return statsObject;
    } catch (error) {
      console.error('Error getting all stats:', error);
      return gameStats;
    }
  };

  const clearAllStats = () => {
    setGameStats({
      blackjack: { wins: 0, losses: 0, pushes: 0, totalWagered: 0, totalWon: 0 },
      poker: { wins: 0, losses: 0, pushes: 0, totalWagered: 0, totalWon: 0 },
      slots: { wins: 0, losses: 0, totalWagered: 0, totalWon: 0 },
      roulette: { wins: 0, losses: 0, totalWagered: 0, totalWon: 0 }
    });
    localStorage.setItem('gameStats', JSON.stringify({}));
  };

  const getSavedGames = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(`${API_URL}/all`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      
      // Convert to format expected by UI
      const games = {};
      response.data.forEach(game => {
        const state = JSON.parse(game.state);
        games[game.gameType] = {
          ...state,
          savedAt: game.updatedAt,
          gameType: game.gameType
        };
      });
      
      setSavedGames(games);
      return Object.values(games);
    } catch (error) {
      console.error('Error getting saved games:', error);
      return Object.values(savedGames);
    }
  };

  return (
    <GameStateContext.Provider value={{
      saveGame,
      loadGame,
      deleteGame,
      hasSavedGame,
      getSavedGames,
      updateGameStats,
      getGameStats,
      getAllStats,
      clearAllStats
    }}>
      {children}
    </GameStateContext.Provider>
  );
};

export default GameStateContext;
