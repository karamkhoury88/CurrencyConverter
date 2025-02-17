# Currency Converter

A robust, scalable, and maintainable currency conversion API using C# and ASP.NET Core, ensuring high performance, security, and resilience.

## Introduction
The Currency Converter API provides reliable and efficient conversion between different currencies. It supports fetching the latest exchange rates, converting amounts, and retrieving historical exchange rates. This API is designed to be scalable, secure, and easy to integrate with other applications.

## Features
- **Fetch Latest Exchange Rates**: Retrieve the latest exchange rates for a specific base currency (e.g., EUR).
- **Convert Amounts**: Convert amounts between different currencies with ease.
- **Historical Exchange Rates**: Retrieve historical exchange rates for a given period with pagination.
- **Swagger API Documentation**: Automatically generated API documentation for easy testing and integration.

## Tech
- **.NET 9**
- **[.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)**: Building observable, production-ready apps.
- **[Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview?tabs=bash)**: A dashboard for comprehensive app monitoring and inspection.
  - This dashboard allows you to closely track various aspects of your app, including logs, traces, and environment configurations, in real-time.
- **[Hybrid Cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0)**: Redis + In Memory.
- **Swagger API Documentation**

## Setup Instructions
- Currency Converter requires [Docker](https://www.docker.com/products/docker-desktop/) to run.
- Currency Converter requires [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).


## Run Instructions
- Ensure to have the following required secrets:
  - `"Kestrel:Certificates:Development:Password": "PASSWORD"`
  - `"CurrencyConverterConfiguration:Jwt:SecretKey": "HS256_Key"`
- Run the `CurrencyConverter.AppHost` project; this will automatically host the API and the Redis cache, and run the Aspire dashboard.

## Assumptions Made
- **Database**: Using an in-memory database for storing user accounts (for development purposes only).
- **Caching**: Using .NET Hybrid Cache for (in-memory and distributed) caching.
- **Authentication**: Using ASP.NET Core Identity for role-based access control (RBAC).
- **Monitoring**: Using OpenTelemetry for distributed tracing, monitoring, and structured logging.
- **API Integration**: Supporting correlating requests against the Frankfurter API.
- **Logging**: Supporting in/out HTTP requests logging.
- **API Versioning**: Implemented for future-proofing.
- **Dependency Injection**: Implemented for service abstractions.
- **Provider Factory**: Dynamically selecting the currency provider based on the request.
- **JWT Tokens**: Using HS256 for JWT tokens.
- **Security**: All currency conversion endpoints are authenticated and authorized.
- **Configuration**: All configuration stored in `appsettings.(Environment).json`.
- **Secrets Management**: All secrets are stored in User Secrets (`secrets.json`) (environment variables are supported).
- **Resilience**: Using .NET [Standard resilience defaults](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli) to globally configure the resiliency of HTTP clients (Rate limiter, handling timeout, retry policies with exponential backoff, Circuit breaker).
- **Rate Limiting**: Implemented for API throttling on two factors:
  - IP Address for all users.
  - User ID for authenticated users.
- **Circuit Breaker**: Implemented to gracefully handle API outages.
- **Testing**: Created a test project for unit tests and integration tests with Mocking.
- **Scalability**: Supported horizontal scaling for handling large request volumes (tested with three instances of the API running).

## Future Enhancements
- Use a real database instead of an in-memory one.
- Increase the test coverage.
- Set up a CI/CD pipeline using tools like GitHub Actions, Azure DevOps, and automate the tests before deployments.
- Automate infrastructure on the cloud by using Infrastructure as Code (e.g., Terraform).
- Ensure zero downtime deployment strategy (e.g., Blue-Green, Canary).
- Use a cloud App configuration system (e.g., Azure App Configuration) to store the configuration.
- Use a cloud secrets management (e.g., Azure Key Vault).
- Set up and configure alerts for critical issues.
- Increase the test coverage to include stress/performance testing.

## Test Coverage Areas
- **Auth Controller**
- **Currency Converter Controller**
- **Currency Converter Factory**
- **Frankfurter Integration Service**
- **Configuration Service**

To generate the coverage report: please run `GenerateCoverageReport.ps1` PowerShell script in the test project.
