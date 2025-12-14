import React from 'react';
import './WarningModal.css';

const WarningModal = ({ show, onConfirm, onCancel, title, message }) => {
  if (!show) return null;

  return (
    <div className="warning-modal-overlay" onClick={onCancel}>
      <div className="warning-modal" onClick={(e) => e.stopPropagation()}>
        <div className="warning-modal-header">
          <h2>{title || 'Warning'}</h2>
        </div>
        <div className="warning-modal-body">
          <p>{message || 'Are you sure you want to leave? You may lose unsaved progress.'}</p>
        </div>
        <div className="warning-modal-footer">
          <button className="btn-cancel" onClick={onCancel}>
            Stay on Page
          </button>
          <button className="btn-confirm" onClick={onConfirm}>
            Leave Anyway
          </button>
        </div>
      </div>
    </div>
  );
};

export default WarningModal;
