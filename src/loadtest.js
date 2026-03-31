import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  
  
  stages: [
    //{ duration: '30s', target: 20 }, // 1. TEST: Normal Load (20 Kullanıcı)
     //{ duration: '30s', target: 50 }, // 2. TEST: Peak Load (50 Kullanıcı)
     { duration: '30s', target: 100 }, // 3. TEST: Stress Load (100 Kullanıcı)
  ],
};

export default function () {
  const baseUrl = 'http://localhost:5173/api/v1'; 

  
  const queryRes = http.get(`${baseUrl}/Flight/query?AirportFrom=IST&AirportTo=IZM&NumberOfPeople=1`);
  
  check(queryRes, {
    'Query Flight is status 200': (r) => r.status === 200,
  });

  
  const checkInPayload = JSON.stringify({
    flightNumber: 'TK123',
    date: '2026-05-01T00:00:00Z',
    passengerName: 'Test Yolcu'
  });
  
  const params = {
    headers: { 'Content-Type': 'application/json' },
  };

  const checkInRes = http.post(`${baseUrl}/CheckIn`, checkInPayload, params);
  
  
  
  sleep(1); 
}