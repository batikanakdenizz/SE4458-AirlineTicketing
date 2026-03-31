import http from 'k6/http';
import { check } from 'k6';

export const options = {
  vus: __ENV.VUS ? parseInt(__ENV.VUS) : 20,
  duration: __ENV.DURATION || '30s',
};

export default function () {
  const url =
    'http://localhost:5173/api/v1/Flight/query' +
    '?airportFrom=ADB' +
    '&airportTo=IST' +
    '&departureDateFrom=2026-04-01T00:00:00Z' +
    '&departureDateTo=2026-04-30T23:59:59Z' +
    '&numberOfPeople=1' +
    '&isRoundTrip=false' +
    '&page=1' +
    '&size=10';

  const res = http.get(url, {
    headers: {
      Accept: 'application/json',
    },
  });

  check(res, {
    'status is 200': (r) => r.status === 200,
  });
}