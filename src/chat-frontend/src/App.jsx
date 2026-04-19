import React, { useState, useCallback } from 'react';
import ChatWindow from './components/ChatWindow';
import ChatInput from './components/ChatInput';

export default function App() {
  const [messages, setMessages] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [sessionId, setSessionId] = useState(null);
  const [error, setError] = useState(null);

  const sendMessage = useCallback(async (text) => {
    if (!text.trim() || isLoading) return;

    const userMsg = { role: 'user', content: text };
    setMessages((prev) => [...prev, userMsg]);
    setIsLoading(true);
    setError(null);

    try {
      const res = await fetch('/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: text, sessionId }),
      });

      if (!res.ok) {
        const errData = await res.json().catch(() => ({}));
        throw new Error(errData.error || `Server error ${res.status}`);
      }

      const data = await res.json();
      setSessionId(data.sessionId);

      const assistantMsg = { role: 'assistant', content: data.content };
      setMessages((prev) => [...prev, assistantMsg]);
    } catch (err) {
      console.error('Chat error:', err);
      setError(err.message);
      setTimeout(() => setError(null), 5000);
    } finally {
      setIsLoading(false);
    }
  }, [isLoading, sessionId]);

  return (
    <div className="app">
      <header className="header">
        <div className="header-logo">✈️</div>
        <div className="header-info">
          <h1>SkyAgent</h1>
          <p>AI-Powered Airline Assistant</p>
        </div>
        <div className="header-status">
          <span className={`status-dot ${isLoading ? '' : ''}`}></span>
          <span>Online</span>
        </div>
      </header>

      <ChatWindow
        messages={messages}
        isLoading={isLoading}
        onSuggestionClick={sendMessage}
      />

      <ChatInput onSend={sendMessage} disabled={isLoading} />

      {error && <div className="error-toast">⚠️ {error}</div>}
    </div>
  );
}
