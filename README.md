# Currency Converter

A robust, scalable, and maintainable currency conversion API using C# and ASP.NET Core, ensuring high performance, security, and resilience.

## Features
- Fetch the latest exchange rates for a specific base currency (e.g., EUR)
- Convert amounts between different currencies
- Retrieve historical exchange rates for a given period with pagination

## Tech
- .NET 9
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview): Building observable, production-ready apps
- [Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview?tabs=bash): A dashboard for comprehensive app monitoring and inspection
  - This dashboard allows you to closely track various aspects of your app, including logs, traces, and environment configurations, in real-time.
- [Hybrid Cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0): Redis + In Memory
- Swagger API documentation

## Setup instructions
- Currency Converter requires [Docker](https://www.docker.com/products/docker-desktop/) to run
- Currency Converter requires [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Currency Converter requires an Integrated Developer Environment (IDE) or code editor (e.g., Visual Studio)

## Run instructions
- Ensure to have the following required secrets:
  - "Kestrel:Certificates:Development:Password": "PASSWORD"
  - "CurrencyConverterConfiguration:Jwt:SecretKey": "HS256_Key"
- Run the `CurrencyConverter.AppHost` project; this will automatically host the API and the Redis cache, and run the Aspire dashboard

## Assumptions made
- I used an in-memory database for storing user accounts (for development purposes only)
- I used .NET Hybrid Cache for (in-memory and distributed) caching
- I used ASP.NET Core Identity for role-based access control (RBAC)
- I used OpenTelemetry for distributed tracing, monitoring, and structured logging
- I supported correlating requests against the Frankfurter API
- I supported in/out HTTP requests logging
- I implemented API versioning for future-proofing
- I implemented dependency injection for service abstractions
- I implemented a provider factory to dynamically select the currency provider based on the request
- I used HS256 for JWT tokens
- All currency conversion endpoints are authenticated and authorized 
- All configuration stored in `appsettings.(Environment).json`
- All secrets are stored in User Secrets (`secrets.json`) (environment variables are supported)
- I used .NET [Standard resilience defaults](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli) to globally configure the resiliency of HTTP clients (Rate limiter, handling timeout, retry policies with exponential backoff, Circuit breaker)
- I implemented Rate Limiting (Fixed window strategy) for API throttling on two factors:
  - IP Address for all users
  - User ID for authenticated users 
- I implemented a circuit breaker to gracefully handle API outages
- I created a test project for unit tests and integration tests with Mocking
- I supported horizontal scaling for handling large request volumes (I tested with three instances of the API running)

## Future enhancements
- Use a real database instead of an in-memory one
- Increase the test coverage
- Set up a CI/CD pipeline using tools like GitHub Actions, Azure DevOps
- Automate infrastructure on the cloud by using Infrastructure as Code (e.g., Terraform)
- Ensure zero downtime deployment strategy (e.g., Blue-Green, Canary)
- Use a cloud App configuration system (e.g., Azure App Configuration) to store the configuration
- Use a cloud secrets management (e.g. Azure Key Vault)
- Set up and configure alerts for critical issues

## Test Coverage Areas
- Auth Controler 
- Currency Converter Conroller
- Currency Converter Factory
- Frankfurter Integration Service
