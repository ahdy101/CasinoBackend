import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { ThemeProvider } from './context/ThemeContext';
import { AuthProvider, useAuth } from './context/AuthContext';
import { GameStateProvider } from './context/GameStateContext';
import Header from './components/layout/Header';
import AdminSidebar from './components/layout/AdminSidebar';
import GameTypeSidebar from './components/layout/GameTypeSidebar';
import Footer from './components/layout/Footer';
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
import SlotsLanding from './pages/Games/SlotsLanding';
import BeatAtlas from './pages/Games/BeatAtlas';
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
  const { isAuthenticated, isLoading, isAdmin } = useAuth();
  const location = useLocation();

  // Show admin sidebar on all admin routes
  const showAdminSidebar = isAuthenticated && isAdmin && location.pathname.startsWith('/admin');
  
  // Show game type sidebar when in game areas (slots, etc.) - but not for admin
  const showGameTypeSidebar = location.pathname.startsWith('/slots') && !isAdmin;
  
  // Hide header nav links when in game areas
  const hideHeaderNav = location.pathname.startsWith('/slots');

  if (isLoading) {
    return (
      <div className="app">
        <div style={{ 
          display: 'flex', 
          justifyContent: 'center', 
          alignItems: 'center', 
          height: '100vh',
          color: 'var(--gold)',
          fontSize: '1.5rem'
        }}>
          Loading...
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 4000,
          style: {
            background: 'var(--bg-secondary)',
            color: 'var(--text-primary)',
            border: '1px solid var(--border-color)',
            borderRadius: 'var(--radius-md)',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
          },
          success: {
            iconTheme: {
              primary: 'var(--gold)',
              secondary: 'white',
            },
          },
          error: {
            iconTheme: {
              primary: 'var(--error)',
              secondary: 'white',
            },
          },
        }}
      />
      {<Header hideNav={hideHeaderNav} />}
      {showAdminSidebar && <AdminSidebar />}
      {showGameTypeSidebar && <GameTypeSidebar />}
      <div className={showAdminSidebar || showGameTypeSidebar ? 'app-content-with-sidebar' : ''}>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/" element={<Navigate to="/lobby" />} />
          <Route path="/lobby" element={<Lobby />} />
          <Route path="/slots" element={<SlotsLanding />} />
          <Route
            path="/slots/beat-atlas"
            element={<ProtectedRoute>
              <BeatAtlas />
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
        <Route path="/about" element={<About />} />
        <Route path="/rules" element={<Rules />} />
        <Route path="/privacy" element={<Privacy />} />
        <Route path="/contact" element={<Contact />} />
        <Route
          path="/admin"
          element={<AdminRoute>
            <AdminDashboard />
          </AdminRoute>} />
      </Routes>
      </div>
      <Footer />
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
