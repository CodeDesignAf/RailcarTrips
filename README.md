# Railcar Trips

Railcar Trips is a take-home implementation that ingests railcar equipment events from CSV, processes trips using domain rules, and exposes results through a REST API with a Blazor WebAssembly UI. The solution is structured with a clean separation between Domain, Application, Infrastructure, API, and Client layers.

The stack uses ASP.NET Core for the API, Blazor WebAssembly for the client, EF Core + SQLite for persistence, and CsvHelper for CSV parsing. The focus is reproducibility, clear architecture boundaries, and deterministic trip processing behavior.

## Run Locally

### Requirements
- .NET 8 SDK

### Steps
```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/RailcarTrips.Api
dotnet run --project src/RailcarTrips.Client
```

### Default URLs
- API Swagger: `http://localhost:5171/swagger`
- Client: `http://localhost:5066`

## Quick Start (One Command)

Windows:
```powershell
./run.ps1
```

Mac/Linux:
```bash
./run.sh
```

These scripts will restore dependencies, build the solution, run tests, and then launch the API and Client.

## How To Use
- Open `http://localhost:5066/railcar-trips`
- Upload `equipment_events.csv`
- The import pipeline processes events and persists events/trips
- Review trips in the grid and inspect trip events from the details panel

## API Endpoints
- `GET /health`
- `POST /api/import/equipment-events`
- `GET /api/trips`
- `GET /api/trips/{tripId}/events`

## High-Level Processing Logic
- Convert event local time to UTC using `City.TimeZoneId`
- Group events by `EquipmentId`
- Sort each equipment group by `EventUtcTime`
- Apply state machine rules: `W` starts a trip, `Z` closes a trip

## Testing
- Unit tests: domain trip-processing behavior (`tests/RailcarTrips.Domain.Tests`)
- Integration tests: import + query flow through API (`tests/RailcarTrips.Api.IntegrationTests`)
- Run all tests with:
```bash
dotnet test
```

## If I Had More Time
- Add richer API integration tests for anomaly-heavy scenarios and edge time-zone cases
- Add city-name caching/projection optimization for query endpoints
- Add lightweight client-side E2E smoke tests for critical user flows
