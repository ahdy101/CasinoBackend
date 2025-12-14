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
    name: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [errors, setErrors] = useState({});
  const [successMessage, setSuccessMessage] = useState('');

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
    
    if (!formData.name) {
      newErrors.name = 'Name is required';
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
    }
    
    if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }
    
    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    const newErrors = validateForm();
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    const result = await register(formData.email, formData.password, formData.name);
    if (result.success) {
      setSuccessMessage(result.message);
      setTimeout(() => {
        navigate('/lobby');
      }, 2000);
    } else {
      setErrors({ general: result.message });
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-content">
        <div className="auth-header">
          <img src={LOGO} alt="The Silver Slayed" className="auth-logo" />
          <h1 className="auth-title">Join The Silver Slayed</h1>
          <p className="auth-subtitle">Create your account and get $15,000 welcome bonus!</p>
        </div>

        <Card className="auth-card">
          {successMessage && (
            <div className="success-banner">
              {successMessage}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <Input
              label="Full Name"
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              placeholder="Enter your name"
              error={errors.name}
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
                <p>Get $15,000 free credits on signup!</p>
              </div>
            </div>

            <Button type="submit" variant="primary" size="large" fullWidth>
              Create Account
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
