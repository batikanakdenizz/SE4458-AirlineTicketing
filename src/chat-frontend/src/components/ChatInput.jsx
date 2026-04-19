import React, { useState, useRef } from 'react';

export default function ChatInput({ onSend, disabled }) {
  const [text, setText] = useState('');
  const inputRef = useRef(null);

  const handleSend = () => {
    if (text.trim() && !disabled) {
      onSend(text.trim());
      setText('');
      inputRef.current?.focus();
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="chat-input-container">
      <div className="chat-input-wrapper">
        <textarea
          ref={inputRef}
          className="chat-input"
          placeholder="Ask about flights, bookings, or check-in..."
          value={text}
          onChange={(e) => setText(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={disabled}
          rows={1}
          id="chat-input"
        />
        <button
          className="send-button"
          onClick={handleSend}
          disabled={disabled || !text.trim()}
          id="send-button"
          aria-label="Send message"
        >
          ➤
        </button>
      </div>
      <div className="chat-input-hint">
        Press Enter to send · Shift+Enter for new line
      </div>
    </div>
  );
}
