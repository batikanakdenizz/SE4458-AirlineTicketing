# ✈️ Airline Ticketing System (SE4458 Midterm Project)

## 📌 Overview

This project is a simplified airline ticketing backend system built using **.NET 8**, designed with a **layered architecture** and extended with an **API Gateway (Ocelot)**.

The system allows users to:
- Search for flights (one-way and round-trip)
- Purchase tickets
- Perform check-in
- Retrieve passenger lists
- Bulk upload flights via CSV
- Authenticate using JWT

Additionally, a **rate-limited API Gateway** is implemented to control request traffic.

---

## 🏗️ Architecture


Client → API Gateway (Ocelot) → Backend API → PostgreSQL


### Layers:
- **Domain** → Entities
- **Application** → DTOs & Interfaces
- **Infrastructure** → Services & DbContext
- **API** → Controllers
- **Gateway** → Routing + Rate Limiting

---

## ⚙️ Technologies Used

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core
- PostgreSQL
- Ocelot API Gateway
- JWT Authentication
- k6 (Load Testing)

---

## 🔐 Authentication

JWT-based authentication is implemented.


## ER Diagram


erDiagram
    USER {
        int Id PK
        string Username
        string PasswordHash
        string Role
    }

    FLIGHT {
        int Id PK
        string FlightNumber
        DateTime DepartureTime
        DateTime ArrivalTime
        string AirportFrom
        string AirportTo
        int DurationMinutes
        int Capacity
        int AvailableSeats
        DateTime CreatedAt
    }

    TICKET {
        int Id PK
        string TicketNumber
        int FlightId FK
        string PassengerName
        DateTime PurchaseDate
        TicketStatus Status
    }

    CHECKIN {
        int Id PK
        int TicketId FK
        int SeatNumber
        DateTime CheckInTime
    }

    FLIGHT ||--o{ TICKET : "1 to Many"
    TICKET ||--o| CHECKIN : "1 to 0..1"

### Login Endpoint:

POST /gateway/auth/login

Example:
```json
{
  "username": "admin",
  "password": "admin123"
}
```

## ✈️ Core Features

1. **Flight Creation**
   - POST `/gateway/flights`
2. **Flight Search (Rate Limited)**
   - GET `/gateway/flights/query`
   - Supports:
     - Date range filtering
     - Passenger count filtering
     - Round-trip search
     - Pagination
3. **Ticket Purchase**
   - POST `/gateway/tickets`
4. **Check-In**
   - POST `/gateway/checkin`
5. **Passenger List**
   - GET `/gateway/flights/passengers`
6. **CSV Flight Upload**
   - POST `/gateway/flights/upload`

- ✔ Duplicate flights are skipped
- ✔ Skipped flight numbers are returned

✔ Duplicate flights are skipped
✔ Skipped flight numbers are returned

🚦 API Gateway (Ocelot)

The system uses Ocelot as an API Gateway to:

Route requests
Apply rate limiting
Provide a single entry point
Example Route:
/gateway/flights/query → /api/v1/Flight/query
⛔ Rate Limiting

Rate limiting is applied on the flight search endpoint.

Configuration:
"RateLimitOptions": {
  "EnableRateLimiting": true,
  "ClientIdHeader": "Oc-Client",
  "Limit": 3,
  "Period": "1m"
}
Important Note:

Ocelot identifies clients using a header, not IP.

Required Header:
Oc-Client: any-value
📊 Load Testing

Load testing was performed using k6.

Test Configuration:
Virtual Users (VUs): 5
Duration: 30 seconds
Endpoint: /gateway/flights/query
Observations:
Initial requests returned 200 OK

After exceeding the limit:

HTTP 429 Too Many Requests
Rate limiting behaved correctly
System remained stable under concurrent load
Sample k6 Script:
import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  vus: 5,
  duration: '30s',
};

export default function () {
  const url = 'http://localhost:5026/gateway/flights/query?...';

  http.get(url, {
    headers: {
      'Oc-Client': 'loadtest-user'
    }
  });

  sleep(1);
}
🧠 Design Decisions
Why Ocelot?
Lightweight API Gateway
Easy configuration
Suitable for educational microservice architecture
Why Header-Based Rate Limiting?

Ocelot's built-in rate limiting works using client headers, not IP-based identification.

Alternative:
ASP.NET Core Rate Limiter → IP-based
Not used to stay aligned with gateway-based design
⚠️ Known Limitations
Rate limiting is header-based, not IP-based
No distributed caching (e.g., Redis)
No frontend UI
Single database instance
🚀 Future Improvements
Redis-based distributed rate limiting
Role-based authorization
Logging & monitoring
Deployment to Azure (App Service + Gateway)
▶️ How to Run
1. Run API
dotnet run --project src/AirlineTicketing.API
2. Run Gateway
dotnet run --project src/AirlineTicketing.Gateway
3. Test via Gateway
http://localhost:5026/gateway/...
👨‍💻 Author

Batıkan Akdeniz
Software Engineering Student
Yaşar University