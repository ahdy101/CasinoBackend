import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from './context/ThemeContext';
import { AuthProvider, useAuth } from './context/AuthContext';
import { GameStateProvider } from './context/GameStateContext';
import Header from './components/layout/Header';
import Login from './pages/Auth/Login';
import Register from './pages/Auth/Register';
import Lobby from './pages/Lobby/Lobby';
import Profile from './pages/Profile/Profile';
import Wallet from './pages/Wallet/Wallet';
import Transactions from './pages/Transactions/Transactions';
import Settings from './pages/Settings/Settings';
import About from './pages/About/About';
import Rules from './pages/Rules/Rules';
import Privacy from './pages/Privacy/Privacy';
import Contact from './pages/Contact/Contact';
import AdminDashboard from './pages/Admin/AdminDashboard';
import Slots from './pages/Games/Slots';
import './styles/theme.css';

// Protected Route Component
const ProtectedRoute = ({ children }) => {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? children : <Navigate to="/login" />;
};

// Admin Route Component
const AdminRoute = ({ children }) => {
  const { isAuthenticated, isAdmin } = useAuth();
  
  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }
  
  if (!isAdmin) {
    return <Navigate to="/lobby" />;
  }
  
  return children;
};

function AppContent() {
  const { isAuthenticated } = useAuth();

  return (
    <div className="app">
      {isAuthenticated && <Header />}
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route
          path="/"
          element={isAuthenticated ? <Navigate to="/lobby" /> : <Navigate to="/login" />} />
        <Route
          path="/lobby"
          element={<ProtectedRoute>
            <Lobby />
          </ProtectedRoute>} />
        <Route
          path="/slots"
          element={<ProtectedRoute>
            <Slots />
          </ProtectedRoute>} />
        <Route
          path="/profile"
          element={<ProtectedRoute>
            <Profile />
          </ProtectedRoute>} />
        <Route
          path="/wallet"
          element={<ProtectedRoute>
            <Wallet />
          </ProtectedRoute>} />
        <Route
          path="/transactions"
          element={<ProtectedRoute>
            <Transactions />
          </ProtectedRoute>} />
        <Route
          path="/settings"
          element={<ProtectedRoute>
            <Settings />
          </ProtectedRoute>} />
        <Route
          path="/about"
          element={<ProtectedRoute>
            <About />
          </ProtectedRoute>} />
        <Route
          path="/rules"
          element={<ProtectedRoute>
            <Rules />
          </ProtectedRoute>} />
        <Route
          path="/privacy"
          element={<ProtectedRoute>
            <Privacy />
          </ProtectedRoute>} />
        <Route
          path="/contact"
          element={<ProtectedRoute>
            <Contact />
          </ProtectedRoute>} />
        <Route
          path="/admin"
          element={<AdminRoute>
            <AdminDashboard />
          </AdminRoute>} />
          </Routes>
        </div>
      );
    }

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <GameStateProvider>
          <Router>
            <AppContent />
          </Router>
        </GameStateProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
