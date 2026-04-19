import { Ollama } from 'ollama';
import OpenAI from 'openai';
import { config } from './config.js';
import { getHistory, addMessage } from './conversationStore.js';
import { getOllamaTools, callTool } from './mcpClient.js';

let ollama;
let openai;

if (config.llmProvider === 'groq') {
  openai = new OpenAI({
    baseURL: 'https://api.groq.com/openai/v1',
    apiKey: config.groqApiKey,
  });
} else {
  ollama = new Ollama({ host: config.ollamaHost });
}

const SYSTEM_PROMPT = `You are an AI assistant for an airline ticketing system. You help users with flight-related tasks through natural conversation.

You have access to the following tools:
1. query_flights - Search for available flights between airports
2. buy_ticket - Purchase ticket(s) for a flight
3. check_in - Check in a passenger for their flight
4. create_booking - Create a full booking (PNR) with passenger details
5. get_booking - Look up a booking by PNR code
6. get_ticket - Look up a ticket by ticket number

Important rules:
- Use IATA airport codes: IST (Istanbul Atatürk), SAW (Istanbul Sabiha Gökçen), ADB (Izmir Adnan Menderes), ESB (Ankara Esenboğa), AYT (Antalya), FRA (Frankfurt), CDG (Paris), LHR (London Heathrow), JFK (New York JFK), etc.
- When the user mentions a city name, convert it to the appropriate IATA code.
- All dates must be in ISO 8601 format (e.g. 2026-06-15T00:00:00Z).
- If the user says "tomorrow", "next week", etc., calculate the actual date. Today is ${new Date().toISOString().split('T')[0]}.
- For departure date ranges, set departureDateFrom to the start of the day (T00:00:00Z) and departureDateTo to the end of the day (T23:59:59Z).
- CRITICAL: BEFORE calling buy_ticket or create_booking, you MUST explicitly ask the user for the passenger's first and last names (and contact email/phone if creating a booking). DO NOT makeup or guess passenger names.
- Ask for any missing required parameters before calling a tool.
- Present results in a clear, readable format.
- After showing flight results, suggest booking if appropriate.
- After booking, suggest check-in if appropriate.
- Be helpful, concise, and conversational.
- If a tool returns an error, explain it to the user in friendly language.`;

/**
 * Convert Ollama tools to OpenAI format.
 */
function getOpenAITools() {
  return getOllamaTools().map(t => ({
    type: 'function',
    function: {
      name: t.function.name,
      description: t.function.description,
      parameters: t.function.parameters
    }
  }));
}

/**
 * Process a user chat message and return the assistant's response.
 * Handles the LLM → tool call → LLM loop.
 */
export async function processMessage(sessionId, userMessage) {
  const history = getHistory(sessionId);

  // Initialize session with system prompt if new
  if (history.length === 0) {
    addMessage(sessionId, { role: 'system', content: SYSTEM_PROMPT });
  }

  // Add user message
  addMessage(sessionId, { role: 'user', content: userMessage });

  let maxIterations = 5; // Prevent infinite tool-calling loops

  while (maxIterations > 0) {
    maxIterations--;
    
    let assistantMessage;
    
    if (config.llmProvider === 'groq') {
      const response = await openai.chat.completions.create({
        model: config.groqModel,
        messages: getHistory(sessionId).map(m => {
          // Format for OpenAI API (remove Ollama specific fields)
          const msg = { role: m.role, content: m.content || "" };
          if (m.tool_calls) msg.tool_calls = m.tool_calls;
          if (m.tool_call_id) msg.tool_call_id = m.tool_call_id;
          if (m.name) msg.name = m.name;
          return msg;
        }),
        tools: getOpenAITools(),
        tool_choice: "auto",
      });
      assistantMessage = response.choices[0].message;
    } else {
      const response = await ollama.chat({
        model: config.ollamaModel,
        messages: getHistory(sessionId).filter(m => m.role !== 'tool'), // Ollama tool results are added differently sometimes, but standard format works
        tools: getOllamaTools(),
        stream: false,
      });
      assistantMessage = response.message;
    }

    // If no tool calls, we have the final response
    if (!assistantMessage.tool_calls || assistantMessage.tool_calls.length === 0) {
      addMessage(sessionId, { role: 'assistant', content: assistantMessage.content || "" });
      return {
        role: 'assistant',
        content: assistantMessage.content || "",
        toolCalls: [],
      };
    }

    // Process tool calls
    addMessage(sessionId, assistantMessage);
    const toolResults = [];

    for (const toolCall of assistantMessage.tool_calls) {
      const toolName = toolCall.function.name;
      // OpenAI returns arguments as string, Ollama as object
      const toolArgsStr = typeof toolCall.function.arguments === 'string' 
        ? toolCall.function.arguments 
        : JSON.stringify(toolCall.function.arguments);
        
      const toolArgs = typeof toolCall.function.arguments === 'string'
        ? JSON.parse(toolCall.function.arguments)
        : toolCall.function.arguments;

      console.log(`Tool call: ${toolName}(${toolArgsStr})`);

      try {
        const result = await callTool(toolName, toolArgs);
        const resultText = result.content?.map(c => c.text).join('\n') || JSON.stringify(result);

        toolResults.push({
          tool: toolName,
          args: toolArgs,
          result: resultText,
          isError: result.isError || false,
        });

        if (config.llmProvider === 'groq') {
          addMessage(sessionId, { 
            role: 'tool', 
            content: resultText,
            tool_call_id: toolCall.id,
            name: toolName
          });
        } else {
          addMessage(sessionId, { role: 'tool', content: resultText });
        }
      } catch (err) {
        const errorMsg = `Tool execution failed: ${err.message}`;
        toolResults.push({ tool: toolName, args: toolArgs, result: errorMsg, isError: true });
        
        if (config.llmProvider === 'groq') {
          addMessage(sessionId, { 
            role: 'tool', 
            content: errorMsg,
            tool_call_id: toolCall.id,
            name: toolName
          });
        } else {
          addMessage(sessionId, { role: 'tool', content: errorMsg });
        }
      }
    }
  }

  // Fallback if max iterations reached
  const fallback = 'I apologize, but I had trouble processing your request. Could you please try again?';
  addMessage(sessionId, { role: 'assistant', content: fallback });
  return { role: 'assistant', content: fallback, toolCalls: [] };
}
