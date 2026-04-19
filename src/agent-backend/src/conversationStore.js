/**
 * In-memory conversation history store.
 * Keyed by session ID, stores message arrays with TTL-based expiry.
 */

const SESSION_TTL_MS = 30 * 60 * 1000; // 30 minutes

const sessions = new Map();

export function getHistory(sessionId) {
  const session = sessions.get(sessionId);
  if (!session) return [];
  session.lastAccess = Date.now();
  return session.messages;
}

export function addMessage(sessionId, message) {
  let session = sessions.get(sessionId);
  if (!session) {
    session = { messages: [], lastAccess: Date.now() };
    sessions.set(sessionId, session);
  }
  session.lastAccess = Date.now();
  session.messages.push(message);

  // Keep only last 30 messages to avoid context overflow
  if (session.messages.length > 30) {
    // Keep the system prompt (first message) and trim old messages
    const systemMsg = session.messages[0]?.role === 'system' ? session.messages[0] : null;
    session.messages = session.messages.slice(-28);
    if (systemMsg && session.messages[0]?.role !== 'system') {
      session.messages.unshift(systemMsg);
    }
  }
}

export function clearSession(sessionId) {
  sessions.delete(sessionId);
}

// Periodic cleanup of expired sessions
setInterval(() => {
  const now = Date.now();
  for (const [id, session] of sessions) {
    if (now - session.lastAccess > SESSION_TTL_MS) {
      sessions.delete(id);
    }
  }
}, 60_000);
