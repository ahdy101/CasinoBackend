import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { MdAccountBalanceWallet, MdLock, MdInfo } from 'react-icons/md';
import { FaCreditCard, FaBitcoin, FaGift, FaCircle } from 'react-icons/fa';
import { SiPaypal } from 'react-icons/si';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';
import './Wallet.css';

const Wallet = () => {
  const { balance, purchaseChips, addTransaction } = useAuth();
  const [selectedPackage, setSelectedPackage] = useState(null);
  const [customAmount, setCustomAmount] = useState('');
  const [message, setMessage] = useState(null);

  const chipPackages = [
    { id: 1, chips: 1000, price: 10, popular: false },
    { id: 2, chips: 5000, price: 45, popular: true, bonus: '10% Bonus' },
    { id: 3, chips: 10000, price: 85, popular: false, bonus: '15% Bonus' },
    { id: 4, chips: 25000, price: 200, popular: false, bonus: '20% Bonus' },
    { id: 5, chips: 50000, price: 375, popular: false, bonus: '25% Bonus' },
    { id: 6, chips: 100000, price: 700, popular: false, bonus: '30% Bonus' }
  ];

  const formatBalance = (amount) => {
    return new Intl.NumberFormat('en-US', {
      style: 'decimal',
      minimumFractionDigits: 0
    }).format(amount);
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  };

  const handlePurchase = (pkg) => {
    // Mock payment processing
    const success = purchaseChips(pkg.chips, pkg.price);
    
    if (success) {
      setMessage({
        type: 'success',
        text: `Successfully purchased ${formatBalance(pkg.chips)} chips!`
      });
      setSelectedPackage(null);
      
      // Add transaction record
      addTransaction({
        type: 'deposit',
        amount: pkg.chips,
        price: pkg.price,
        status: 'completed'
      });
    } else {
      setMessage({
        type: 'error',
        text: 'Payment failed. Please try again.'
      });
    }

    setTimeout(() => setMessage(null), 5000);
  };

  const handleCustomPurchase = () => {
    const chips = parseInt(customAmount);
    if (isNaN(chips) || chips < 100) {
      setMessage({
        type: 'error',
        text: 'Minimum purchase is 100 chips'
      });
      return;
    }

    const price = chips / 100; // $1 = 100 chips
    const success = purchaseChips(chips, price);
    
    if (success) {
      setMessage({
        type: 'success',
        text: `Successfully purchased ${formatBalance(chips)} chips!`
      });
      setCustomAmount('');
      
      addTransaction({
        type: 'deposit',
        amount: chips,
        price: price,
        status: 'completed'
      });
    }

    setTimeout(() => setMessage(null), 5000);
  };

  return (
    <div className="wallet-container">
      <div className="wallet-header">
        <div className="wallet-balance-section">
          <h1 className="wallet-title"><MdAccountBalanceWallet /> My Wallet</h1>
          <div className="current-balance">
            <span className="balance-label">Current Balance</span>
            <span className="balance-value">{formatBalance(balance)}</span>
            <span className="balance-type">In-Game Chips</span>
          </div>
        </div>
      </div>

      {message && (
        <div className={`wallet-message ${message.type}`}>
          {message.text}
        </div>
      )}

      <div className="wallet-info">
        <Card className="info-card">
          <div className="info-icon"><MdLock /></div>
          <h3>Safe & Secure</h3>
          <p>All transactions are encrypted and secure. We never store your payment information.</p>
        </Card>
        <Card className="info-card">
          <div className="info-icon"><FaCircle /></div>
          <h3>In-Game Currency</h3>
          <p>Chips are used for playing games. 100 chips = $1 USD</p>
        </Card>
        <Card className="info-card">
          <div className="info-icon"><FaGift /></div>
          <h3>Bonus Rewards</h3>
          <p>Larger purchases receive bonus chips automatically!</p>
        </Card>
      </div>

      <section className="chip-packages-section">
        <h2 className="section-title">Purchase Chip Packages</h2>
        <div className="packages-grid">
          {chipPackages.map(pkg => (
            <Card 
              key={pkg.id} 
              className={`package-card ${pkg.popular ? 'popular' : ''} ${selectedPackage?.id === pkg.id ? 'selected' : ''}`}
              onClick={() => setSelectedPackage(pkg)}
            >
              {pkg.popular && <div className="popular-badge">Most Popular</div>}
              {pkg.bonus && <div className="bonus-badge">{pkg.bonus}</div>}
              <div className="package-chips">
                <span className="chip-icon"><FaCircle /></span>
                <span className="chip-amount">{formatBalance(pkg.chips)}</span>
                <span className="chip-label">Chips</span>
              </div>
              <div className="package-price">{formatPrice(pkg.price)}</div>
              <Button 
                onClick={(e) => {
                  e.stopPropagation();
                  handlePurchase(pkg);
                }}
                className="purchase-button"
              >
                Purchase Now
              </Button>
            </Card>
          ))}
        </div>
      </section>

      <section className="custom-purchase-section">
        <Card className="custom-purchase-card">
          <h2 className="section-title">Custom Amount</h2>
          <p className="section-description">
            Purchase a custom amount of chips (Minimum: 100 chips)
          </p>
          <div className="custom-purchase-form">
            <Input
              type="number"
              value={customAmount}
              onChange={(e) => setCustomAmount(e.target.value)}
              placeholder="Enter chip amount"
              min="100"
            />
            <div className="custom-price">
              {customAmount && !isNaN(parseInt(customAmount)) && parseInt(customAmount) >= 100 && (
                <span>Price: {formatPrice(parseInt(customAmount) / 100)}</span>
              )}
            </div>
            <Button onClick={handleCustomPurchase} disabled={!customAmount || parseInt(customAmount) < 100}>
              Purchase Custom Amount
            </Button>
          </div>
        </Card>
      </section>

      <section className="payment-methods">
        <h2 className="section-title">Accepted Payment Methods</h2>
        <div className="payment-icons">
          <div className="payment-icon"><FaCreditCard /> Credit Card</div>
          <div className="payment-icon"><FaCreditCard /> Debit Card</div>
          <div className="payment-icon"><SiPaypal /> PayPal</div>
          <div className="payment-icon"><FaBitcoin /> Crypto</div>
        </div>
        <p className="payment-note">
          <MdInfo /> Your payment information is processed securely and never stored on our servers.
        </p>
      </section>
    </div>
  );
};

export default Wallet;
