import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { LOGO } from '../../constants/images';
import Input from '../../components/common/Input';
import Button from '../../components/common/Button';
import Card from '../../components/common/Card';
import './Auth.css';

const Register = () => {
  const navigate = useNavigate();
  const { register } = useAuth();
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [errors, setErrors] = useState({});
  const [successMessage, setSuccessMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
    if (errors[e.target.name]) {
      setErrors({ ...errors, [e.target.name]: '' });
    }
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.username) {
      newErrors.username = 'Username is required';
    }
    
    if (!formData.email) {
      newErrors.email = 'Email is required';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Email is invalid';
    }
    
    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters';
    } else {
      // Check password strength requirements
      const hasUpperCase = /[A-Z]/.test(formData.password);
      const hasLowerCase = /[a-z]/.test(formData.password);
      const hasNumber = /[0-9]/.test(formData.password);
      const hasSpecialChar = /[@$!%*?&]/.test(formData.password);
      
      if (!hasUpperCase || !hasLowerCase || !hasNumber || !hasSpecialChar) {
        newErrors.password = 'Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&)';
      }
    }
    
    if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }
    
    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    console.log('Form submitted with data:', formData);
    
    const newErrors = validateForm();
    if (Object.keys(newErrors).length > 0) {
      console.log('Validation errors:', newErrors);
      setErrors(newErrors);
      return;
    }

    setLoading(true);
    setErrors({});

    try {
      console.log('Calling register API...');
      const result = await register(formData.username, formData.email, formData.password);
      console.log('Register result:', result);
      
      if (result.success) {
        setSuccessMessage(result.message);
        setTimeout(() => {
          navigate('/lobby');
        }, 2000);
      } else {
        setErrors({ general: result.message });
      }
    } catch (error) {
      console.error('Registration error:', error);
      setErrors({ general: 'Network error. Please check if the API server is running.' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-content">
        <div className="auth-header">
          <img src={LOGO} alt="The Silver Slayed" className="auth-logo" />
          <h1 className="auth-title">Join The Silver Slayed</h1>
          <p className="auth-subtitle">Create your account and get $10,000 welcome bonus!</p>
        </div>

        <Card className="auth-card">
          {errors.general && (
            <div className="error-banner" style={{ 
              backgroundColor: '#fee', 
              color: '#c33', 
              padding: '10px', 
              borderRadius: '4px', 
              marginBottom: '15px',
              border: '1px solid #fcc'
            }}>
              {errors.general}
            </div>
          )}
          
          {successMessage && (
            <div className="success-banner">
              {successMessage}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <Input
              label="Username"
              type="text"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="Choose a username"
              error={errors.username}
              required
            />

            <Input
              label="Email"
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="Enter your email"
              error={errors.email}
              required
            />

            <Input
              label="Password"
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Create a password"
              error={errors.password}
              required
            />
            <p style={{ 
              fontSize: '0.85rem', 
              color: '#888', 
              marginTop: '-10px', 
              marginBottom: '15px' 
            }}>
              Must include uppercase, lowercase, number, and special character (@$!%*?&)
            </p>

            <Input
              label="Confirm Password"
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Confirm your password"
              error={errors.confirmPassword}
              required
            />

            <div className="welcome-bonus-banner">
              <span className="bonus-icon">GIFT</span>
              <div className="bonus-text">
                <strong>Welcome Bonus</strong>
                <p>Get $10,000 free credits on signup!</p>
              </div>
            </div>

            <Button type="submit" variant="primary" size="large" fullWidth disabled={loading}>
              {loading ? 'Creating Account...' : 'Create Account'}
            </Button>
          </form>

          <div className="auth-divider">
            <span>or</span>
          </div>

          <p className="auth-register">
            Already have an account?{' '}
            <Link to="/login" className="auth-link-primary">
              Login here
            </Link>
          </p>
        </Card>
      </div>
    </div>
  );
};

export default Register;
