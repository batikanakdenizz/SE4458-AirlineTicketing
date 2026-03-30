import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  // Hocanızın istediği en az 30 saniyelik test senaryoları.
  // Testi yaparken sadece bir tanesinin başındaki // (yorum) işaretini kaldırın, diğerlerini kapatın.
  
  stages: [
    //{ duration: '30s', target: 20 }, // 1. TEST: Normal Load (20 Kullanıcı)
     //{ duration: '30s', target: 50 }, // 2. TEST: Peak Load (50 Kullanıcı)
     { duration: '30s', target: 100 }, // 3. TEST: Stress Load (100 Kullanıcı)
  ],
};

export default function () {
  const baseUrl = 'http://localhost:5173/api/v1'; // Buradaki portu API'nizin asıl portu ile değiştirin

  // 1. ENDPOINT: Uçuş Arama (Yetki Gerektirmez)
  const queryRes = http.get(`${baseUrl}/Flight/query?AirportFrom=IST&AirportTo=IZM&NumberOfPeople=1`);
  
  check(queryRes, {
    'Query Flight is status 200': (r) => r.status === 200,
  });

  // 2. ENDPOINT: Check-in İşlemi (Yetki Gerektirmez)
  const checkInPayload = JSON.stringify({
    flightNumber: 'TK123',
    date: '2026-05-01T00:00:00Z',
    passengerName: 'Test Yolcu'
  });
  
  const params = {
    headers: { 'Content-Type': 'application/json' },
  };

  const checkInRes = http.post(`${baseUrl}/CheckIn`, checkInPayload, params);
  
  // Not: Bilet gerçekten olmadığı için 400 Bad Request dönebilir, sorun değil. 
  // Önemli olan sunucunun bu yük altında yanıt verebilmesidir.
  
  sleep(1); // Her simüle edilen kullanıcının istekler arası bekleme süresi
}