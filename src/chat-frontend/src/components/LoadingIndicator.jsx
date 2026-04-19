import React from 'react';

export default function LoadingIndicator() {
  return (
    <div className="loading-row">
      <div className="message-avatar" style={{ background: 'linear-gradient(135deg, #3b82f6, #06b6d4)' }}>
        ✈️
      </div>
      <div className="loading-bubble">
        <div className="loading-dot"></div>
        <div className="loading-dot"></div>
        <div className="loading-dot"></div>
      </div>
    </div>
  );
}
