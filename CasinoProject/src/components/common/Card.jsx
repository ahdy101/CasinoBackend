import React from 'react';
import './Card.css';

const Card = ({ 
  children, 
  className = '', 
  hover = true,
  onClick
}) => {
  return (
    <div 
      className={`card ${hover ? 'card-hover' : ''} ${className}`}
      onClick={onClick}
      style={{ cursor: onClick ? 'pointer' : 'default' }}
    >
      {children}
    </div>
  );
};

export default Card;
