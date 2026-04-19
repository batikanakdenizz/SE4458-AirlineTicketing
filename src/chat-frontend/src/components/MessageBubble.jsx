import React from 'react';

export default function MessageBubble({ message }) {
  const { role, content } = message;

  return (
    <div className={`message-row ${role}`}>
      {role === 'assistant' && (
        <div className="message-avatar">✈️</div>
      )}
      <div className="message-bubble">
        {content}
      </div>
    </div>
  );
}
