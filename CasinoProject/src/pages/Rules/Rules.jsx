import React from 'react';
import Card from '../../components/common/Card';
import './Rules.css';

const Rules = () => {
  return (
    <div className="rules-container">
      <div className="rules-header">
        <h1 className="rules-title">Game Rules</h1>
        <p className="rules-subtitle">Learn how to play our casino games</p>
      </div>

      <div className="rules-content">
        <Card className="rules-card">
          <h2>Blackjack</h2>
          <p>Get as close to 21 as possible without going over. Beat the dealer's hand to win.</p>
          <ul>
            <li>Face cards are worth 10 points</li>
            <li>Aces are worth 1 or 11 points</li>
            <li>Dealer must hit on 16 and stand on 17</li>
            <li>Blackjack pays 3:2</li>
          </ul>
        </Card>

        <Card className="rules-card">
          <h2>Poker</h2>
          <p>Texas Hold'em - Make the best 5-card hand using your 2 cards and 5 community cards.</p>
          <ul>
            <li>Royal Flush is the highest hand</li>
            <li>High Card is the lowest hand</li>
            <li>Best hand wins the pot</li>
          </ul>
        </Card>

        <Card className="rules-card">
          <h2>Roulette</h2>
          <p>Bet on where the ball will land on the spinning wheel.</p>
          <ul>
            <li>Single number pays 35:1</li>
            <li>Red/Black pays 1:1</li>
            <li>Odd/Even pays 1:1</li>
            <li>Multiple betting options available</li>
          </ul>
        </Card>

        <Card className="rules-card">
          <h2>Slots</h2>
          <p>Match symbols across the reels to win.</p>
          <ul>
            <li>3 matching symbols wins</li>
            <li>Triple 7s is the jackpot</li>
            <li>Bet amount affects payout</li>
          </ul>
        </Card>

        <Card className="rules-card">
          <h2>General Rules</h2>
          <ul>
            <li>Must be 18+ to play</li>
            <li>All bets are final</li>
            <li>Play responsibly</li>
            <li>House edge applies to all games</li>
          </ul>
        </Card>
      </div>
    </div>
  );
};

export default Rules;
