import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';import { MdHistory, MdTrendingUp, MdTrendingDown } from 'react-icons/md';
import { FaCoins } from 'react-icons/fa6';import Card from '../../components/common/Card';
import './Transactions.css';

const Transactions = () => {
  const { transactions } = useAuth();
  const [filter, setFilter] = useState('all'); // all, deposits, withdrawals

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatAmount = (amount) => {
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

  const getStatusColor = (status) => {
    switch (status) {
      case 'completed':
        return 'success';
      case 'pending':
        return 'warning';
      case 'failed':
        return 'danger';
      default:
        return '';
    }
  };

  const filteredTransactions = transactions?.filter(t => {
    if (filter === 'all') return true;
    if (filter === 'deposits') return t.type === 'deposit';
    if (filter === 'withdrawals') return t.type === 'withdrawal';
    return true;
  }) || [];

  const totalDeposits = transactions?.filter(t => t.type === 'deposit' && t.status === 'completed')
    .reduce((sum, t) => sum + t.amount, 0) || 0;
  
  const totalWithdrawals = transactions?.filter(t => t.type === 'withdrawal' && t.status === 'completed')
    .reduce((sum, t) => sum + t.amount, 0) || 0;

  return (
    <div className="transactions-container">
      <h1 className="transactions-title"><MdHistory /> Transaction History</h1>

      <div className="transactions-summary">
        <Card className="summary-card">
          <div className="summary-icon deposit"><MdTrendingUp /></div>
          <div className="summary-info">
            <span className="summary-label">Total Deposits</span>
            <span className="summary-value success">+{formatAmount(totalDeposits)}</span>
            <span className="summary-sublabel">Chips</span>
          </div>
        </Card>
        <Card className="summary-card">
          <div className="summary-icon withdrawal"><MdTrendingDown /></div>
          <div className="summary-info">
            <span className="summary-label">Total Withdrawals</span>
            <span className="summary-value danger">-{formatAmount(totalWithdrawals)}</span>
            <span className="summary-sublabel">Chips</span>
          </div>
        </Card>
        <Card className="summary-card">
          <div className="summary-icon net"><FaCoins /></div>
          <div className="summary-info">
            <span className="summary-label">Net Balance</span>
            <span className={`summary-value ${totalDeposits >= totalWithdrawals ? 'success' : 'danger'}`}>
              {totalDeposits >= totalWithdrawals ? '+' : ''}{formatAmount(totalDeposits - totalWithdrawals)}
            </span>
            <span className="summary-sublabel">Chips</span>
          </div>
        </Card>
      </div>

      <Card className="transactions-card">
        <div className="transactions-filters">
          <button 
            className={`filter-button ${filter === 'all' ? 'active' : ''}`}
            onClick={() => setFilter('all')}
          >
            All Transactions
          </button>
          <button 
            className={`filter-button ${filter === 'deposits' ? 'active' : ''}`}
            onClick={() => setFilter('deposits')}
          >
            Deposits
          </button>
          <button 
            className={`filter-button ${filter === 'withdrawals' ? 'active' : ''}`}
            onClick={() => setFilter('withdrawals')}
          >
            Withdrawals
          </button>
        </div>

        <div className="transactions-list">
          {filteredTransactions.length > 0 ? (
            <div className="transactions-table">
              <div className="table-header">
                <div className="table-col">Date & Time</div>
                <div className="table-col">Type</div>
                <div className="table-col">Amount</div>
                <div className="table-col">Price</div>
                <div className="table-col">Status</div>
                <div className="table-col">ID</div>
              </div>
              {filteredTransactions.map((transaction) => (
                <div key={transaction.id} className="table-row">
                  <div className="table-col">
                    <span className="transaction-date">{formatDate(transaction.timestamp)}</span>
                  </div>
                  <div className="table-col">
                    <span className={`transaction-type ${transaction.type}`}>
                      {transaction.type === 'deposit' ? 'Deposit' : 'Withdrawal'}
                    </span>
                  </div>
                  <div className="table-col">
                    <span className={`transaction-amount ${transaction.type === 'deposit' ? 'success' : 'danger'}`}>
                      {transaction.type === 'deposit' ? '+' : '-'}{formatAmount(transaction.amount)} chips
                    </span>
                  </div>
                  <div className="table-col">
                    <span className="transaction-price">{formatPrice(transaction.price)}</span>
                  </div>
                  <div className="table-col">
                    <span className={`transaction-status ${getStatusColor(transaction.status)}`}>
                      {transaction.status.charAt(0).toUpperCase() + transaction.status.slice(1)}
                    </span>
                  </div>
                  <div className="table-col">
                    <span className="transaction-id">{transaction.id}</span>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="no-transactions">
              <div className="no-transactions-icon">EMPTY</div>
              <h3>No Transactions Yet</h3>
              <p>Your transaction history will appear here once you make a purchase.</p>
            </div>
          )}
        </div>
      </Card>

      <Card className="transaction-info">
        <h3>Transaction Information</h3>
        <ul>
          <li><strong>Deposits:</strong> Purchase in-game chips with real money (100 chips = $1 USD)</li>
          <li><strong>Withdrawals:</strong> Convert chips back to real money (Coming Soon)</li>
          <li><strong>Transaction ID:</strong> Unique identifier for tracking and support</li>
          <li><strong>Security:</strong> All transactions are encrypted and secure</li>
          <li><strong>Support:</strong> Contact us if you have questions about any transaction</li>
        </ul>
      </Card>
    </div>
  );
};

export default Transactions;
