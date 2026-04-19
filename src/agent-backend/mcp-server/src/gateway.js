import axios from 'axios';

const GATEWAY_URL = process.env.GATEWAY_URL || 'http://localhost:5010';
const AUTH_USERNAME = process.env.AUTH_USERNAME || 'admin';
const AUTH_PASSWORD = process.env.AUTH_PASSWORD || 'admin123';

let jwtToken = null;

/**
 * Authenticate with the gateway and cache the JWT token.
 */
async function authenticate() {
  console.error(`[gateway] Authenticating as ${AUTH_USERNAME} at ${GATEWAY_URL}`);
  const res = await axios.post(`${GATEWAY_URL}/gateway/auth/login`, {
    username: AUTH_USERNAME,
    password: AUTH_PASSWORD,
  });
  jwtToken = res.data.token;
  console.error('[gateway] Authentication successful');
  return jwtToken;
}

/**
 * Get a valid JWT token, authenticating if necessary.
 */
async function getToken() {
  if (!jwtToken) {
    await authenticate();
  }
  return jwtToken;
}

/**
 * Common headers for all requests (includes Client header for rate limiting).
 */
function commonHeaders() {
  return { Client: 'skyagent-mcp' };
}

/**
 * Make an authenticated request. Retries once on 401.
 */
async function authRequest(method, url, data, params) {
  const token = await getToken();
  const headers = { ...commonHeaders(), Authorization: `Bearer ${token}` };
  try {
    console.error(`[gateway] ${method.toUpperCase()} ${url}`);
    const res = await axios({ method, url: `${GATEWAY_URL}${url}`, data, params, headers, timeout: 30000 });
    return res.data;
  } catch (err) {
    if (err.response?.status === 401) {
      await authenticate();
      const retryHeaders = { ...commonHeaders(), Authorization: `Bearer ${jwtToken}` };
      const res = await axios({ method, url: `${GATEWAY_URL}${url}`, data, params, headers: retryHeaders, timeout: 30000 });
      return res.data;
    }
    console.error(`[gateway] ERROR ${method.toUpperCase()} ${url}: ${err.response?.status} ${err.response?.data?.message || err.message}`);
    throw err;
  }
}

/**
 * Make a public (unauthenticated) request.
 */
async function publicRequest(method, url, data, params) {
  const headers = commonHeaders();
  try {
    console.error(`[gateway] ${method.toUpperCase()} ${url}`);
    const res = await axios({ method, url: `${GATEWAY_URL}${url}`, data, params, headers, timeout: 30000 });
    return res.data;
  } catch (err) {
    console.error(`[gateway] ERROR ${method.toUpperCase()} ${url}: ${err.response?.status} ${err.response?.data?.message || err.message}`);
    throw err;
  }
}

// ── Tool handlers ──────────────────────────────────────────────

export async function queryFlights({ airportFrom, airportTo, departureDateFrom, departureDateTo, numberOfPeople }) {
  return publicRequest('get', '/gateway/flights/query', null, {
    AirportFrom: airportFrom,
    AirportTo: airportTo,
    DepartureDateFrom: departureDateFrom,
    DepartureDateTo: departureDateTo,
    NumberOfPeople: numberOfPeople || 1,
    Page: 1,
    Size: 10,
  });
}

export async function buyTicket({ flightNumber, departureDate, passengerNames }) {
  return authRequest('post', '/gateway/tickets', {
    flightNumber,
    departureDate,
    passengerNames,
  });
}

export async function checkIn({ flightNumber, departureDate, passengerName }) {
  return publicRequest('post', '/gateway/checkin', {
    flightNumber,
    departureDate,
    passengerName,
  });
}

export async function createBooking({ flightNumber, departureDate, contactEmail, contactPhone, totalAmount, currency, passengers }) {
  return publicRequest('post', '/gateway/bookings', {
    flightNumber,
    departureDate,
    contactEmail: contactEmail || 'passenger@example.com',
    contactPhone: contactPhone || null,
    totalAmount: totalAmount || 0,
    currency: currency || 'TRY',
    passengers: passengers || [],
  });
}

export async function getBooking({ pnrCode }) {
  return authRequest('get', `/gateway/bookings/${pnrCode}`);
}

export async function getTicket({ ticketNumber }) {
  return authRequest('get', `/gateway/tickets/${ticketNumber}`);
}
