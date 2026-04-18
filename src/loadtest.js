import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: __ENV.VUS ? parseInt(__ENV.VUS) : 20 },
  ],
};

export default function () {
  const baseUrl = `${__ENV.BASE_URL || 'http://localhost:5173'}/api/v1`;

  const queryRes = http.get(`${baseUrl}/Flight/query?airportFrom=IST&airportTo=IZM&numberOfPeople=1`);

  check(queryRes, {
    'Query Flight is status 200': (r) => r.status === 200,
  });

  const checkInPayload = JSON.stringify({
    flightNumber: __ENV.FLIGHT_NUMBER || 'TK123',
    departureDate: __ENV.DEPARTURE_DATE || '2026-05-01T00:00:00Z',
    passengerName: __ENV.PASSENGER_NAME || 'Test Passenger',
  });

  const params = {
    headers: { 'Content-Type': 'application/json' },
  };

  const checkInRes = http.post(`${baseUrl}/CheckIn`, checkInPayload, params);

  check(checkInRes, {
    'Check-in did not crash': (r) => r.status < 500,
  });

  sleep(1);
}
