# SE 4458 – ASSIGNMENT 2

## 1. Create an AI Agent chat application for the Query Flight, Book Flight and Check in APIs you created in Midterm

### Students

- BATIKAN AKDENİZ

---


## EXPECTED ARCHITECTURE

For chat application, use a web frontend framework like React, Flutter, Angular or a mobile frontend like React-native. Flutter.

- Make sure all your API calls go through the gateway
- Consider using Firestore or Realtime Database for Real Time Messaging. See below architecture recommendation
- Call a Firestore cloud function or another API to Call LLM API (OpenAI or local model like Mistral, Ollama) to parse intent and parameters  
  See examples at https://github.com/southriver/SE4458-AIAgent
- Develop an MCP server for your APIs
- LLM decides which MCP tool to call.
- MCP server maps that tool to the correct gateway endpoint.
- Gateway routes to your midterm API.
- Call Midterm APIs per message text. Assume your chat application uses constant userid/password for authentication when needed. You can add more APIs if you wish
- Refresh chat API per API responses

You can also utilize websockets with a backend WebSocket Server or Server Send Events with required libraries

---

## Simple implementation idea

### Components

- Frontend: React or Flutter chat UI
- Agent backend: Node.js / Python / Java
- LLM: OpenAI, Ollama, Mistral, etc.
- MCP server: Node or Python
- Gateway: your existing API gateway
- Midterm APIs: airline or listing services

Frontend chat app  
Agent backend / LLM  
MCP client  
MCP server  
API Gateway  
Midterm APIs

---

## Minimal backend logic

- receive chat message
- send message + conversation history to LLM
- allow LLM to call MCP tools
- tool calls hit gateway endpoints
- return response to frontend

---

## DELIVERABLES

- Project does NOT need to be deployed to a cloud app service if you are using a local LLM

- Link to your github code
  - A readme document in your github code repo that has:
    - code link to source code of the project i.e github, bitbucket
    - your design, assumptions, and issues you encountered.
    - Include a link to a short video presenting your project (hosted on one drive, google drive, youtube)

---

## Example

Code  
https://github.com/southriver/apiNode  

Deployment  
https://mpurfkzikk.eu-central-1.awsapprunner.com/api-docs/