import React, { useRef, useEffect } from 'react';
import MessageBubble from './MessageBubble';
import LoadingIndicator from './LoadingIndicator';

const SUGGESTIONS = [
  '✈️ Find flights from Istanbul to Frankfurt',
  '🎫 Book flight TK100 for tomorrow',
  '🪪 Check in for my flight',
  '📋 Look up booking ABC123',
];

export default function ChatWindow({ messages, isLoading, onSuggestionClick }) {
  const endRef = useRef(null);

  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isLoading]);

  return (
    <div className="chat-window" id="chat-window">
      {messages.length === 0 && !isLoading && (
        <div className="welcome">
          <div className="welcome-icon">✈️</div>
          <h2>Welcome to SkyAgent</h2>
          <p>
            I can help you search for flights, book tickets, and check in for your journey. 
            Just type your request in natural language!
          </p>
          <div className="welcome-suggestions">
            {SUGGESTIONS.map((s, i) => (
              <button
                key={i}
                className="suggestion-chip"
                onClick={() => onSuggestionClick(s)}
              >
                {s}
              </button>
            ))}
          </div>
        </div>
      )}

      {messages.map((msg, i) => (
        <MessageBubble key={i} message={msg} />
      ))}

      {isLoading && <LoadingIndicator />}

      <div ref={endRef} />
    </div>
  );
}
