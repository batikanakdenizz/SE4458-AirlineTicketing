import { Client } from '@modelcontextprotocol/sdk/client/index.js';
import { StdioClientTransport } from '@modelcontextprotocol/sdk/client/stdio.js';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const MCP_SERVER_PATH = path.resolve(__dirname, '../../mcp-server/src/index.js');

let client = null;
let cachedTools = null;

/**
 * Initialize the MCP client by spawning the MCP server as a child process.
 */
export async function initMcpClient() {
  if (client) return;

  const transport = new StdioClientTransport({
    command: 'node',
    args: [MCP_SERVER_PATH],
    env: {
      ...process.env,
      GATEWAY_URL: process.env.GATEWAY_URL || 'http://localhost:5010',
      AUTH_USERNAME: process.env.AUTH_USERNAME || 'admin',
      AUTH_PASSWORD: process.env.AUTH_PASSWORD || 'admin123',
    },
  });

  client = new Client({ name: 'airline-agent', version: '1.0.0' });
  await client.connect(transport);
  console.log('MCP client connected to airline-ticketing-mcp server');

  // Pre-fetch tool list
  const { tools } = await client.listTools();
  cachedTools = tools;
  console.log(`Discovered ${tools.length} MCP tools: ${tools.map(t => t.name).join(', ')}`);
}

/**
 * Get the list of available MCP tools.
 */
export function getMcpTools() {
  return cachedTools || [];
}

/**
 * Convert MCP tools to Ollama-compatible tool format.
 */
export function getOllamaTools() {
  return getMcpTools().map((tool) => ({
    type: 'function',
    function: {
      name: tool.name,
      description: tool.description,
      parameters: tool.inputSchema,
    },
  }));
}

/**
 * Execute an MCP tool by name with given arguments.
 */
export async function callTool(name, args) {
  if (!client) throw new Error('MCP client not initialized');
  const result = await client.callTool({ name, arguments: args });
  return result;
}
