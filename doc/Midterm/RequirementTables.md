# SE 4458 Software Architecture & Design of Modern Large Scale Systems - Midterm

## Group 1 – API Project for Airline Company

Create an API project that will perform below requirements for an airline ticketing system

---

## API Requirements

| API | Parameters | API Response | Description |
|-----|-----------|--------------|-------------|
| Add Flight | Flight number, datefrom, date-to, airportfrom, airport-to, duration, capacity | Transaction status | Adds a flight to airline schedule |
| Add Flight by File | .csv file with following fields: Flight number, datefrom, date-to, airportfrom, airport-to, duration, capacity | File processes status | Adds all the flights in the file |
| Query Flight | date-from, date-to, airport-from, airport-to, number of people, one way/round trip | List of available flights (Flight number, duration) | Flights that have no seats should not be listed. Limit calls to 3 per day |
| Buy ticket | Flight number, Date, Passenger Name(s) | Transaction status, ticket number | Capacity of flight will be decreased. Return sold out if there are no seats left |
| Check in | Flight number, Date, Passenger Name | Transaction status | Assign seat (simple numbering) to Passenger on flight |
| Query Flight Passenger List | Flight number, Date | List of passengers of seats | |

---

## Other API requirements

| API | Authentication | Paging (size of 10) |
|-----|---------------|---------------------|
| Add Flight | YES | NO |
| Query Flight | NO | YES |
| Buy ticket | YES | NO |
| Check in | NO | NO |
| Query Flight Passenger List | YES | YES |
| Add Flight by File | YES | NO |

---


## COMMON REQUIREMENTS

- You are only asked to develop APIs that will be test in their swaggers. NO FRONT END necessary
- You are required to develop per service oriented principles discussed in class (ex. no Database work in APIs, prefer creating services and usage of DTOs). Not doing so will deduct points
- Every student will do their own midterm, no groups
- All REST services must be versionable. Other API frameworks like GraphQL are also allowed
- Services must support paging, authentication as described.
- For authentication, JWT or Oauth can be implemented. Please check the examples from class
- All APIs must have Swagger UI or document
- You can choose any development environment you like as long as they support REST services.
- You can make assumptions as long as you document them
- create a data model and use a database service from any cloud service you like.
- For API hosting, use cloud service Azure, AWS or Google Cloud (Hosting services like Render and Vercel are not allowed). Points will be deducted if you cant deploy your project to a hosting provider.
- You need to implement an API gateway and configure all apis in the gateway.

### API Gateway

- Rate limiting should be implemented in the API gateway.
- You can implement your own gateway (see https://github.com/southriver/ApiGateway2 ) or use services from Azure/AWS.

---

## Additional Requirement – Load Testing

Students are required to perform basic load testing of their APIs using a tool like k6 to evaluate system performance under concurrent usage.

The objective is to verify that the API services behave correctly and maintain acceptable response times under simulated load.

### Load Testing Requirements

Each student must:

1. Select at least two API endpoints from their project (for example: Query Flight, Buy Ticket, Query Listing, or Book a Stay).
2. Perform load testing using by simulating concurrent users.
3. Run tests under at least three load scenarios:

- Normal Load – 20 virtual users
- Peak Load – 50 virtual users
- Stress Load – 100 virtual users

Each test should run for at least 30 seconds.

---

## Metrics to Collect

Students must record the following metrics from the load testing results:

- Average response time
- 95th percentile response time (p95)
- Number of requests per second
- Error rate (failed requests)

---

## Reporting

Students must include the following in their project repository (README or report):

1. Description of the endpoints tested
2. test scripts used for load testing
3. Screenshots or graphs of the load test results
4. A short analysis (3–5 sentences) explaining:
   - how the API performed under load
   - any observed bottlenecks
   - potential improvements to scalability

### Tools

- https://k6.io/
- Jmeter
- grafana

---

## DELIVERABLES

- Link to your github code
  - A readme document in your github code repo that has:
    - your design, assumptions, and issues you encountered.
    - Data model (i.e an ER)
    - Load Test Results – this could be in Readme or link to a page
    - Include a link to a short video presenting your project (hosted on google drive, youtube)

- Link to your deployed swagger url

---

## Example

TodoApi Code source  
https://github.com/southriver/apiNode  

TodoApi Sample code deployed  
https://mpurfkzikk.eu-central-1.awsapprunner.com/api-docs/

---

## Resources for creating REST services in different environments

### .NET
- https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-webapi?view=aspnetcore-7.0&tabs=visual-studio-code
- https://davidgiard.com/deploying-a-web-app-to-azure-from-visual-studio-code

### Azure Deployment
- Make sure you choose F1 Free version in Azure for App Service that you will be creating
- https://youtu.be/DUfPaY6FRII

### AWS Deployment
- https://www.youtube.com/watch?v=PKbAyADayZE

### PYTHON – Using flask
- https://dev.to/mursalfk/setup-flask-on-windows-system-using-vs-code-4p9j

### JAVA - Host a Spring Boot application
- https://www.baeldung.com/rest-with-spring-series
- https://javawhizz.com/2023/03/host-a-spring-boot-application-for-free-onrender