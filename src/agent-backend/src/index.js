import express from 'express';
import cors from 'cors';
import { v4 as uuidv4 } from 'uuid';
import { config } from './config.js';
import { initMcpClient } from './mcpClient.js';
import { processMessage } from './agent.js';

import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();
app.use(cors());
app.use(express.json());

// Serve static frontend files
app.use(express.static(path.join(__dirname, '../public')));

// ── POST /api/chat ─────────────────────────────────────────────
app.post('/api/chat', async (req, res) => {
  try {
    const { message, sessionId } = req.body;

    if (!message || typeof message !== 'string') {
      return res.status(400).json({ error: 'Message is required' });
    }

    const sid = sessionId || uuidv4();
    console.log(`[${sid}] User: ${message}`);

    const response = await processMessage(sid, message);
    console.log(`[${sid}] Assistant: ${response.content?.substring(0, 100)}...`);

    return res.json({
      sessionId: sid,
      ...response,
    });
  } catch (err) {
    console.error('Chat error:', err);
    return res.status(500).json({
      error: 'Internal server error',
      details: err.message,
    });
  }
});

// ── GET /api/health ────────────────────────────────────────────
app.get('/api/health', (_req, res) => {
  res.json({ status: 'ok', provider: config.llmProvider, model: config.ollamaModel });
});

// ── Catch-all for React ──────────────────────────────────────────
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, '../public/index.html'));
});

// ── Start ──────────────────────────────────────────────────────
async function main() {
  console.log('Initializing MCP client...');
  await initMcpClient();

  app.listen(config.port, () => {
    console.log(`Agent backend running on http://localhost:${config.port}`);
    console.log(`LLM: ${config.llmProvider} (${config.ollamaModel})`);
    console.log(`Gateway: ${config.gatewayUrl}`);
  });
}

main().catch((err) => {
  console.error('Failed to start agent backend:', err);
  process.exit(1);
});
