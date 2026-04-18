import http from 'k6/http';
import { check } from 'k6';

export const options = {
  vus: __ENV.VUS ? parseInt(__ENV.VUS) : 20,
  duration: __ENV.DURATION || '30s',
};

export default function () {
  const baseUrl = __ENV.BASE_URL || 'http://localhost:5173';
  const uniqueId = `${__VU}-${__ITER}-${Date.now()}`;

  const payload = JSON.stringify({
    flightNumber: __ENV.FLIGHT_NUMBER,
    departureDate: __ENV.DEPARTURE_DATE,
    passengerNames: [
      `Passenger-${uniqueId}`
    ]
  });

  const token = __ENV.TOKEN;

  const res = http.post(`${baseUrl}/api/v1/Ticket`, payload, {
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
  });

  check(res, {
    'status is 200': (r) => r.status === 200,
  });
}
