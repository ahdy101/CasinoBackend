import React from 'react';
import Card from '../../components/common/Card';
import './About.css';

const About = () => {
  return (
    <div className="about-container">
      <div className="about-header">
        <h1 className="about-title">About The Silver Slayed</h1>
        <p className="about-subtitle">Your Premier Online Casino Experience</p>
      </div>

      <div className="about-content">
        <Card className="about-card">
          <h2>Our Story</h2>
          <p>
            The Silver Slayed was founded with a vision to bring the excitement and luxury 
            of Las Vegas directly to your screen. We combine cutting-edge technology with 
            classic casino games to create an unforgettable gaming experience.
          </p>
        </Card>

        <Card className="about-card">
          <h2>Our Mission</h2>
          <p>
            To provide a safe, fair, and entertaining gaming environment where players can 
            enjoy their favorite casino games with confidence. We're committed to responsible 
            gaming and ensuring every player has the best possible experience.
          </p>
        </Card>

        <Card className="about-card">
          <h2>Why Choose Us</h2>
          <ul>
            <li>Fair and transparent gaming</li>
            <li>Secure payment processing</li>
            <li>24/7 customer support</li>
            <li>Premium game selection</li>
            <li>Instant withdrawals</li>
          </ul>
        </Card>
      </div>
    </div>
  );
};

export default About;
