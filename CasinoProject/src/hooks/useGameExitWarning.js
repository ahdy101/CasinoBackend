import { useEffect, useCallback, useRef } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

/**
 * Custom hook to prevent navigation when game is active
 * @param {boolean} isGameActive - Whether a game is currently in progress
 * @param {function} onNavigateAttempt - Callback when user tries to navigate away
 * @returns {object} - Helper functions to control the blocker
 */
export const useGameExitWarning = (isGameActive, onNavigateAttempt) => {
  const location = useLocation();
  const navigate = useNavigate();
  const navigationAttemptRef = useRef(null);

  // Prevent browser refresh/close
  useEffect(() => {
    const handleBeforeUnload = (e) => {
      if (isGameActive) {
        e.preventDefault();
        e.returnValue = 'You have an active game. Are you sure you want to leave?';
        return e.returnValue;
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);

    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [isGameActive]);

  // Intercept link clicks within the app
  useEffect(() => {
    const handleClick = (e) => {
      if (!isGameActive) return;

      // Check if clicked element is a link or inside a link
      const link = e.target.closest('a');
      if (link && link.href) {
        const href = link.getAttribute('href');
        
        // Only intercept internal navigation
        if (href && href.startsWith('/')) {
          e.preventDefault();
          navigationAttemptRef.current = href;
          
          if (onNavigateAttempt) {
            onNavigateAttempt(href);
          }
        }
      }
    };

    document.addEventListener('click', handleClick, true);

    return () => {
      document.removeEventListener('click', handleClick, true);
    };
  }, [isGameActive, onNavigateAttempt]);

  const confirmNavigation = useCallback(() => {
    if (navigationAttemptRef.current) {
      navigate(navigationAttemptRef.current);
      navigationAttemptRef.current = null;
    }
  }, [navigate]);

  const cancelNavigation = useCallback(() => {
    navigationAttemptRef.current = null;
  }, []);

  return {
    confirmNavigation,
    cancelNavigation,
    pendingNavigation: navigationAttemptRef.current
  };
};

export default useGameExitWarning;
