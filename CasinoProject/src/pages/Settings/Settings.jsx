import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { MdSettings, MdInfo, MdLock } from 'react-icons/md';
import Card from '../../components/common/Card';
import Input from '../../components/common/Input';
import Button from '../../components/common/Button';
import './Settings.css';

const Settings = () => {
  const { user, updateUserSettings, changePassword } = useAuth();
  const [activeTab, setActiveTab] = useState('account');
  const [accountData, setAccountData] = useState({
    name: user?.name || '',
    email: user?.email || '',
  });
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [preferences, setPreferences] = useState({
    notifications: user?.settings?.notifications ?? true,
    emailUpdates: user?.settings?.emailUpdates ?? true,
    soundEffects: user?.settings?.soundEffects ?? true,
    animations: user?.settings?.animations ?? true,
    language: user?.settings?.language || 'en'
  });
  const [message, setMessage] = useState(null);

  const handleAccountSubmit = (e) => {
    e.preventDefault();
    updateUserSettings({ ...accountData });
    setMessage({ type: 'success', text: 'Account settings updated successfully!' });
    setTimeout(() => setMessage(null), 3000);
  };

  const handlePasswordSubmit = (e) => {
    e.preventDefault();
    
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setMessage({ type: 'error', text: 'New passwords do not match!' });
      return;
    }
    
    if (passwordData.newPassword.length < 6) {
      setMessage({ type: 'error', text: 'Password must be at least 6 characters!' });
      return;
    }

    changePassword(passwordData.currentPassword, passwordData.newPassword);
    setPasswordData({ currentPassword: '', newPassword: '', confirmPassword: '' });
    setMessage({ type: 'success', text: 'Password changed successfully!' });
    setTimeout(() => setMessage(null), 3000);
  };

  const handlePreferencesSubmit = (e) => {
    e.preventDefault();
    updateUserSettings({ settings: preferences });
    setMessage({ type: 'success', text: 'Preferences saved successfully!' });
    setTimeout(() => setMessage(null), 3000);
  };

  return (
    <div className="settings-container">
      <h1 className="settings-title"><MdSettings /> Settings</h1>
      
      {message && (
        <div className={`settings-message ${message.type}`}>
          {message.text}
        </div>
      )}

      <div className="settings-tabs">
        <button 
          className={`tab-button ${activeTab === 'account' ? 'active' : ''}`}
          onClick={() => setActiveTab('account')}
        >
          Account
        </button>
        <button 
          className={`tab-button ${activeTab === 'security' ? 'active' : ''}`}
          onClick={() => setActiveTab('security')}
        >
          Security
        </button>
        <button 
          className={`tab-button ${activeTab === 'preferences' ? 'active' : ''}`}
          onClick={() => setActiveTab('preferences')}
        >
          Preferences
        </button>
      </div>

      <div className="settings-content">
        {activeTab === 'account' && (
          <Card className="settings-card">
            <h2 className="card-title">Account Information</h2>
            <form onSubmit={handleAccountSubmit} className="settings-form">
              <Input
                label="Full Name"
                type="text"
                value={accountData.name}
                onChange={(e) => setAccountData({ ...accountData, name: e.target.value })}
                placeholder="Enter your full name"
              />
              <Input
                label="Email Address"
                type="email"
                value={accountData.email}
                onChange={(e) => setAccountData({ ...accountData, email: e.target.value })}
                placeholder="Enter your email"
              />
              <div className="form-note">
                <span><MdInfo /></span>
                <p>Your email is used for account recovery and important notifications.</p>
              </div>
              <Button type="submit">Save Changes</Button>
            </form>
          </Card>
        )}

        {activeTab === 'security' && (
          <Card className="settings-card">
            <h2 className="card-title">Change Password</h2>
            <form onSubmit={handlePasswordSubmit} className="settings-form">
              <Input
                label="Current Password"
                type="password"
                value={passwordData.currentPassword}
                onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                placeholder="Enter current password"
              />
              <Input
                label="New Password"
                type="password"
                value={passwordData.newPassword}
                onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                placeholder="Enter new password"
              />
              <Input
                label="Confirm New Password"
                type="password"
                value={passwordData.confirmPassword}
                onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                placeholder="Confirm new password"
              />
              <div className="form-note">
                <span><MdLock /></span>
                <p>Password must be at least 6 characters long.</p>
              </div>
              <Button type="submit">Change Password</Button>
            </form>

            <div className="security-section">
              <h3>Two-Factor Authentication</h3>
              <p className="security-description">
                Add an extra layer of security to your account (Coming Soon)
              </p>
              <Button variant="secondary" disabled>Enable 2FA</Button>
            </div>
          </Card>
        )}

        {activeTab === 'preferences' && (
          <Card className="settings-card">
            <h2 className="card-title">User Preferences</h2>
            <form onSubmit={handlePreferencesSubmit} className="settings-form">
              <div className="preference-section">
                <h3>Notifications</h3>
                <div className="toggle-group">
                  <label className="toggle-item">
                    <div>
                      <strong>Push Notifications</strong>
                      <p>Receive notifications for game results and bonuses</p>
                    </div>
                    <input
                      type="checkbox"
                      checked={preferences.notifications}
                      onChange={(e) => setPreferences({ ...preferences, notifications: e.target.checked })}
                      className="toggle-checkbox"
                    />
                  </label>
                  <label className="toggle-item">
                    <div>
                      <strong>Email Updates</strong>
                      <p>Receive promotional emails and updates</p>
                    </div>
                    <input
                      type="checkbox"
                      checked={preferences.emailUpdates}
                      onChange={(e) => setPreferences({ ...preferences, emailUpdates: e.target.checked })}
                      className="toggle-checkbox"
                    />
                  </label>
                </div>
              </div>

              <div className="preference-section">
                <h3>User Experience</h3>
                <div className="toggle-group">
                  <label className="toggle-item">
                    <div>
                      <strong>Sound Effects</strong>
                      <p>Enable game sound effects</p>
                    </div>
                    <input
                      type="checkbox"
                      checked={preferences.soundEffects}
                      onChange={(e) => setPreferences({ ...preferences, soundEffects: e.target.checked })}
                      className="toggle-checkbox"
                    />
                  </label>
                  <label className="toggle-item">
                    <div>
                      <strong>Animations</strong>
                      <p>Enable smooth animations and transitions</p>
                    </div>
                    <input
                      type="checkbox"
                      checked={preferences.animations}
                      onChange={(e) => setPreferences({ ...preferences, animations: e.target.checked })}
                      className="toggle-checkbox"
                    />
                  </label>
                </div>
              </div>

              <div className="preference-section">
                <h3>Language</h3>
                <select 
                  value={preferences.language}
                  onChange={(e) => setPreferences({ ...preferences, language: e.target.value })}
                  className="language-select"
                >
                  <option value="en">English</option>
                  <option value="es">Español</option>
                  <option value="fr">Français</option>
                  <option value="de">Deutsch</option>
                </select>
              </div>

              <Button type="submit">Save Preferences</Button>
            </form>
          </Card>
        )}
      </div>
    </div>
  );
};

export default Settings;
