import dotenv from 'dotenv';
dotenv.config();

export const config = {
  port: parseInt(process.env.PORT || '3001', 10),
  gatewayUrl: process.env.GATEWAY_URL || 'http://localhost:5010',
  authUsername: process.env.AUTH_USERNAME || 'admin',
  authPassword: process.env.AUTH_PASSWORD || 'admin123',
  ollamaHost: process.env.OLLAMA_HOST || 'http://localhost:11434',
  ollamaModel: process.env.OLLAMA_MODEL || 'mistral',
  llmProvider: process.env.LLM_PROVIDER || 'ollama',
  groqApiKey: process.env.GROQ_API_KEY || '',
  groqModel: process.env.GROQ_MODEL || 'llama-3.3-70b-versatile',
};
