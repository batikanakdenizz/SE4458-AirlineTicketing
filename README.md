# ✈️ Service Oriented Airline Ticketing System (SE4458 Midterm Project)

---

## 📌 Project Overview

This project is a backend API system for an airline ticketing platform developed using **.NET 8** and deployed on **Microsoft Azure**.

The system supports core airline operations such as:

* Flight creation (single & CSV bulk)
* Flight search with filtering
* Ticket purchasing
* Passenger check-in
* Passenger listing
* Authentication & authorization
* API Gateway with rate limiting
* Load testing under concurrent usage

The project is designed following **service-oriented and layered architecture principles** as required by the course.

---

## 🌐 Live Deployment

* **Swagger API:**
  https://api-midterm-bgareudhf2aaakar.francecentral-01.azurewebsites.net/

* **API Gateway:**
  https://gateway-midterm-begsgfcubdhxaph0.francecentral-01.azurewebsites.net

---

## 🎥 Project Demo Video

[video link](https://www.youtube.com/watch?v=CF8uzmwN_rc)

---

## 🏗️ Architecture

The system follows a **layered architecture** combined with an **API Gateway pattern**.

Client → API Gateway (Ocelot) → Backend API → PostgreSQL

### Layers

1. **API Layer**

   * ASP.NET Core Controllers
   * Handles HTTP requests & responses

2. **Application Layer**

   * DTOs
   * Interfaces
   * Business contracts

3. **Infrastructure Layer**

   * Entity Framework Core
   * PostgreSQL integration
   * Service implementations

4. **Domain Layer**

   * Core entities:

     * Flight
     * Ticket
     * CheckIn
     * User

---

## 🧬ER Diagram

  erDiagram
    
  LIGHT ||--o{ TICKET : has
  TICKET ||--o| CHECKIN : generates
  USER ||--o{ TICKET : purchases

  FLIGHT {
        int Id PK
        string FlightNumber UK
        datetime DepartureTime
        datetime ArrivalTime
        string AirportFrom
        string AirportTo
        int DurationMinutes
        int Capacity
        int AvailableSeats
        datetime CreatedAt
    }

  TICKET {
        int Id PK
        string TicketNumber UK
        int FlightId FK
        int UserId FK
        string PassengerName
        datetime PurchaseDate
        int Status
    }

  CHECKIN {
        int Id PK
        int TicketId FK_UK
        string SeatNumber
        datetime CheckInTime
    }

  USER {
        int Id PK
        string Username UK
        string PasswordHash
        string Role
    }

---

## 🧱 Technology Stack

* .NET 8 (ASP.NET Core Web API)
* C#
* Entity Framework Core
* PostgreSQL
* JWT Authentication
* Ocelot API Gateway
* k6 (Load Testing)
* Azure App Service

---

## 🔐 Authentication

JWT-based authentication is used.

### Protected Endpoints

* POST /gateway/flights
* POST /gateway/flights/upload
* POST /gateway/tickets
* GET /gateway/flights/passengers

### Public Endpoints

* GET /gateway/flights/query
* POST /gateway/checkin

---

## 🔗 API Gateway Routes

| Gateway Path           | Downstream Path      |
| ---------------------- | -------------------- |
| /gateway/flights       | /api/v1/Flight       |
| /gateway/flights/query | /api/v1/Flight/query |
| /gateway/tickets       | /api/v1/Ticket       |
| /gateway/checkin       | /api/v1/CheckIn      |

---

## 🚦 Rate Limiting

Rate limiting is applied on:

GET /gateway/flights/query

* Limit: 3 requests per minute (demo)
* Header required: `Oc-Client`

Production configuration:

* Limit: 3 requests per day

---

## 📄 Paging

Paging is implemented for endpoints that return lists.

### Supported Endpoints

* Query Flight
* Passenger List

### Parameters

* page (default: 1)
* pageSize (default: 10)

### Example

GET /gateway/flights/query?page=1&pageSize=10

---

## 📡 API Endpoints

* Add Flight → POST /api/v1/Flight
* Query Flight → GET /api/v1/Flight/query
* Buy Ticket → POST /api/v1/Ticket
* Check-In → POST /api/v1/CheckIn
* Passenger List → GET /api/v1/Flight/passengers
* Upload Flights → POST /api/v1/Flight/upload

### Example Request

```json
{
  "flightNumber": "TK2026",
  "departureAirport": "IST",
  "arrivalAirport": "ESB",
  "departureTime": "2026-04-01T10:00:00",
  "arrivalTime": "2026-04-01T11:00:00",
  "price": 1500,
  "capacity": 180
}
```

---

## 🗄️ Data Model

The system is based on a relational data model.

### Main Entities

* Flight
* Ticket
* Passenger
* CheckIn
* User

### Relationships

* A Flight has many Tickets
* A Ticket belongs to a Passenger
* A Passenger can Check-In
* Check-In assigns a seat

📌 ER Diagram (add image here):

```
Flight ───< Ticket ─── Passenger ─── CheckIn
```

---

## 📌 Assumptions

* Flight uniqueness is defined by (flightNumber + date)
* Seat assignment is sequential (1,2,3...)
* No payment system is implemented (simplified)
* CSV files are assumed to be valid format
* Flights with no available seats are excluded from search results

---

## 🧪 Testing

Tools used:

* Swagger UI
* Postman
* k6 Load Testing

### Example Header for Gateway

```
Client: batikan
```

---

## 📊 Load Testing

### 🎯 Objective

Evaluate system performance under concurrent load.

### Tested Endpoints

* Query Flight (read-heavy)
* Buy Ticket (write-heavy)

### Load Scenarios

* 20 VUs → Normal Load
* 50 VUs → Peak Load
* 100 VUs → Stress Load

Duration: 30 seconds

---

## 📈 Load Test Results

| Test         | VUs | Avg(ms) | p95(ms) | RPS    | Error |
| ------------ | --- | ------- | ------- | ------ | ----- |
| Query Flight | 20  | 19.3    | 44.2    | 1028.9 | 0%    |
| Query Flight | 50  | 51.6    | 90.5    | 963.7  | 0%    |
| Query Flight | 100 | 97.7    | 140.0   | 1019.2 | 0%    |
| Buy Ticket   | 20  | 24.0    | 72.0    | 830.9  | 0%    |
| Buy Ticket   | 50  | 67.1    | 209.9   | 743.8  | 0%    |
| Buy Ticket   | 100 | 147.7   | 450.2   | 674.5  | 0%    |

---

## 📈 Load Test Analysis

The system performs efficiently under normal and peak loads with low latency and zero error rates.

Under stress load (100 VUs), response times increase but remain within acceptable limits.

The Buy Ticket endpoint shows higher latency due to database write operations.

Potential improvements include:

* Database indexing
* Horizontal scaling
* Caching frequently queried data

---

## 🧪 k6 Test Script

```javascript
import http from 'k6/http';

export default function () {
  http.get('https://gateway-midterm.../gateway/flights/query', {
    headers: {
      'Client': 'batikan'
    }
  });
}
```

---

## 📊 Response Time Distribution (ms)

```text
Query Flight
ms
│
│            *
│         *
│      *
│   *
│ *
└────────────────────
   20   50   100 VUs
   
Buy Ticket
ms
│
│              *
│          *
│      *
│   *
│ *
└────────────────────
   20   50   100 VUs
```

## 🚀 Throughput (Requests per Second)

```text
Query Flight
req/sec
│
│   ██████████████████████ (~1029)
│   ███████████████████    (~964)
│   █████████████████████  (~1019)
│
└────────────────────────
    20     50     100

Buy Ticket
req/sec
│
│   █████████████████     (~831)
│   █████████████         (~744)
│   ███████████           (~674)
│
└────────────────────────
    20     50     100
```

---

## ⚠️ Issues Encountered

* Azure deployment errors (fixed with proper publish configuration)
* Gateway routing issues (fixed using Azure API URL)
* Rate limiting errors (resolved by adding required header)

---

## 🧠 Design Decisions

* **Ocelot API Gateway** → lightweight and easy to configure
* **Layered Architecture** → maintainability and scalability
* **PostgreSQL** → strong relational consistency
* **JWT Authentication** → stateless security
* **Azure Deployment** → cloud-native scalability

---

## ☁️ Azure Deployment

* Backend API → Azure App Service
* API Gateway → Azure App Service
* Database → PostgreSQL

### Important Configurations

* Environment variables
* HTTPS enforcement
* Ocelot downstream routing

---

