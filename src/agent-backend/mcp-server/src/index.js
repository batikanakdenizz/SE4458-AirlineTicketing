import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { z } from 'zod';
import * as gateway from './gateway.js';

const server = new McpServer({
  name: 'airline-ticketing-mcp',
  version: '1.0.0',
});

// ── Tool 1: Query Flights ──────────────────────────────────────

server.tool(
  'query_flights',
  'Search for available flights between two airports on given dates. Returns matching flights with schedule and seat availability.',
  {
    airportFrom: z.string().describe('IATA code of the departure airport, e.g. IST, ADB, FRA, JFK'),
    airportTo: z.string().describe('IATA code of the arrival airport'),
    departureDateFrom: z.string().describe('Start of departure date range in ISO 8601 format, e.g. 2026-06-15T00:00:00Z'),
    departureDateTo: z.string().describe('End of departure date range in ISO 8601 format, e.g. 2026-06-15T23:59:59Z'),
    numberOfPeople: z.number().optional().default(1).describe('Number of passengers (default 1)'),
  },
  async (params) => {
    try {
      const result = await gateway.queryFlights(params);
      return { content: [{ type: 'text', text: JSON.stringify(result, null, 2) }] };
    } catch (err) {
      const msg = err.response?.data?.message || err.message;
      return { content: [{ type: 'text', text: `Error querying flights: ${msg}` }], isError: true };
    }
  }
);

// ── Tool 2: Buy Ticket ─────────────────────────────────────────

server.tool(
  'buy_ticket',
  'Purchase ticket(s) for a specific flight. Requires flight number, departure date, and list of passenger names.',
  {
    flightNumber: z.string().describe('Flight number, e.g. TK1523'),
    departureDate: z.string().describe('Departure date in ISO 8601 format, e.g. 2026-06-15T00:00:00Z'),
    passengerNames: z.array(z.string()).describe('Array of passenger full names, e.g. ["John Doe", "Jane Doe"]'),
  },
  async (params) => {
    try {
      const result = await gateway.buyTicket(params);
      return { content: [{ type: 'text', text: JSON.stringify(result, null, 2) }] };
    } catch (err) {
      const msg = err.response?.data?.message || err.message;
      return { content: [{ type: 'text', text: `Error buying ticket: ${msg}` }], isError: true };
    }
  }
);

// ── Tool 3: Check In ───────────────────────────────────────────

server.tool(
  'check_in',
  'Check in a passenger for their flight. Assigns a seat number.',
  {
    flightNumber: z.string().describe('Flight number, e.g. TK1523'),
    departureDate: z.string().describe('Departure date in ISO 8601 format'),
    passengerName: z.string().describe('Full name of the passenger exactly as used when buying the ticket'),
  },
  async (params) => {
    try {
      const result = await gateway.checkIn(params);
      return { content: [{ type: 'text', text: JSON.stringify(result, null, 2) }] };
    } catch (err) {
      const msg = err.response?.data?.message || err.message;
      return { content: [{ type: 'text', text: `Error during check-in: ${msg}` }], isError: true };
    }
  }
);

// ── Tool 4: Create Booking ─────────────────────────────────────

server.tool(
  'create_booking',
  'Create a full booking (PNR) with passengers, tickets, and payment. Returns a PNR code.',
  {
    flightNumber: z.string().describe('Flight number, e.g. TK100'),
    departureDate: z.string().describe('Departure date in ISO 8601 format'),
    contactEmail: z.string().describe('Contact email for the booking'),
    contactPhone: z.string().optional().describe('Contact phone number'),
    totalAmount: z.number().describe('Total payment amount'),
    currency: z.string().optional().default('TRY').describe('Currency code (default TRY)'),
    passengers: z.array(z.object({
      firstName: z.string().describe('Passenger first name'),
      lastName: z.string().describe('Passenger last name'),
      dateOfBirth: z.string().optional().describe('Date of birth ISO 8601'),
      documentNumber: z.string().optional().describe('Passport or ID number'),
      nationality: z.string().optional().describe('Nationality code, e.g. TUR'),
    })).describe('List of passengers'),
  },
  async (params) => {
    try {
      const result = await gateway.createBooking(params);
      return { content: [{ type: 'text', text: JSON.stringify(result, null, 2) }] };
    } catch (err) {
      const msg = err.response?.data?.message || err.message;
      return { content: [{ type: 'text', text: `Error creating booking: ${msg}` }], isError: true };
    }
  }
);

// ── Tool 5: Get Booking ────────────────────────────────────────

server.tool(
  'get_booking',
  'Look up a booking by its PNR code. Returns booking details including passengers and tickets.',
  {
    pnrCode: z.string().describe('The PNR code of the booking, e.g. ABC123'),
  },
  async (params) => {
    try {
      const result = await gateway.getBooking(params);
      return { content: [{ type: 'text', text: JSON.stringify(result, null, 2) }] };
    } catch (err) {
      const msg = err.response?.data?.message || err.message;
      return { content: [{ type: 'text', text: `Error fetching booking: ${msg}` }], isError: true };
    }
  }
);

// ── Tool 6: Get Ticket ─────────────────────────────────────────

server.tool(
  'get_ticket',
  'Look up a ticket by its ticket number. Returns ticket details including flight info and status.',
  {
    ticketNumber: z.string().describe('The ticket number, e.g. TKT-A7B3C2'),
  },
  async (params) => {
    try {
      const result = await gateway.getTicket(params);
      return { content: [{ type: 'text', text: JSON.stringify(result, null, 2) }] };
    } catch (err) {
      const msg = err.response?.data?.message || err.message;
      return { content: [{ type: 'text', text: `Error fetching ticket: ${msg}` }], isError: true };
    }
  }
);

// ── Start server ───────────────────────────────────────────────

async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error('Airline MCP Server running on stdio');
}

main().catch((err) => {
  console.error('MCP Server failed to start:', err);
  process.exit(1);
});
