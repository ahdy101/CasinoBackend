import React from 'react';
import Card from '../../components/common/Card';
import './Privacy.css';

const Privacy = () => {
  return (
    <div className="privacy-container">
      <div className="privacy-header">
        <h1 className="privacy-title">Privacy Policy</h1>
        <p className="privacy-subtitle">Your privacy and security are our top priority</p>
      </div>

      <div className="privacy-content">
        <Card className="privacy-card">
          <h2>Information We Collect</h2>
          <p>
            We collect information necessary to provide you with a secure gaming experience, 
            including account details, transaction history, and gameplay data.
          </p>
        </Card>

        <Card className="privacy-card">
          <h2>How We Use Your Data</h2>
          <ul>
            <li>To process transactions securely</li>
            <li>To verify your identity</li>
            <li>To improve our services</li>
            <li>To comply with legal requirements</li>
            <li>To prevent fraud and abuse</li>
          </ul>
        </Card>

        <Card className="privacy-card">
          <h2>Data Security</h2>
          <p>
            All data is encrypted using industry-standard SSL technology. We never store your 
            payment information on our servers, and all transactions are processed through 
            secure, certified payment gateways.
          </p>
        </Card>

        <Card className="privacy-card">
          <h2>Your Rights</h2>
          <ul>
            <li>Access your personal data</li>
            <li>Request data deletion</li>
            <li>Opt-out of marketing communications</li>
            <li>Update your information</li>
          </ul>
        </Card>

        <Card className="privacy-card">
          <h2>Contact Us</h2>
          <p>
            If you have any questions about our privacy policy, please contact our support team.
          </p>
        </Card>
      </div>
    </div>
  );
};

export default Privacy;
