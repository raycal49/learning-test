# Logwatch

A full-stack incident monitoring and root cause analysis platform built to demonstrate production-grade engineering practices. It ingests logs from distributed services, detects anomalies using statistical analysis, groups related errors into incidents, and surfaces everything through a real-time dashboard.

Think of it as a simplified Datadog — built from scratch.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Simulated Services                       │
│         payment-service  auth-service  order-service         │
└───────────────────────┬─────────────────────────────────────┘
                        │ POST /api/logs (batch)
                        ▼
┌───────────────────────────────────────────────────────────┐
│                    ASP.NET Core API                        │
│              Validation → RabbitMQ Publisher               │
└───────────────────────┬───────────────────────────────────┘
                        │ logs.ingest queue
                        ▼
┌───────────────────────────────────────────────────────────┐
│                   Background Worker                        │
│    Consume → Persist → Cluster → Anomaly Detect → Alert    │
└──────┬──────────────────────────────────────┬─────────────┘
       │                                      │
       ▼                                      ▼
┌─────────────┐                    ┌──────────────────────┐
│  PostgreSQL  │                   │     Redis (cache)     │
│  Logs        │                   └──────────────────────┘
│  Incidents   │
│  Alerts      │
└──────┬───────┘
       │ REST API
       ▼
┌───────────────────────────────────────────────────────────┐
│                   React Dashboard                          │
│   Log volume chart · Error rates · Incidents · Alerts      │
└───────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 8, Clean Architecture |
| Background processing | .NET Worker Service |
| Message broker | RabbitMQ |
| Primary database | PostgreSQL 16 + EF Core 8 |
| Caching | Redis 7 |
| Frontend | React 18, TypeScript, Vite |
| Charts | Recharts |
| Styling | Tailwind CSS |
| Containerization | Docker + Docker Compose |
| CI/CD | GitHub Actions |
| Testing | xUnit, Moq, FluentAssertions, Testcontainers |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
cd YOUR_REPO_NAME
```

### 2. Start infrastructure

```bash
docker compose up postgres redis rabbitmq -d
```

Wait about 10 seconds for the containers to become healthy:

```bash
docker ps
```

All three containers should show `(healthy)` in the status column.

### 3. Run the database migration

```bash
dotnet ef database update \
  --project src/IncidentMonitor.Infrastructure/IncidentMonitor.Infrastructure.csproj \
  --startup-project src/IncidentMonitor.API/IncidentMonitor.API.csproj
```

On Windows (PowerShell), replace `\` with a backtick `` ` ``.

### 4. Install frontend dependencies

```bash
cd frontend
npm install
cd ..
```

### 5. Run everything

You'll need five terminals open simultaneously:

**Terminal 1 — API**
```bash
dotnet run --project src/IncidentMonitor.API/IncidentMonitor.API.csproj
```

**Terminal 2 — Worker**
```bash
dotnet run --project src/IncidentMonitor.Worker/IncidentMonitor.Worker.csproj
```

**Terminal 3 — Simulator** (generates live log traffic)
```bash
dotnet run --project src/IncidentMonitor.Simulator/IncidentMonitor.Simulator.csproj
```

**Terminal 4 — Frontend**
```bash
cd frontend
npm run dev
```

**Terminal 5 — Open the dashboard**

Navigate to [http://localhost:5173](http://localhost:5173)

Let the simulator run for 2-3 minutes to build up enough data for the charts to populate.

---

## Running with Docker Compose

To run the full stack in containers:

```bash
docker compose up --build
```

The dashboard will be available at [http://localhost:3000](http://localhost:3000) and the API at [http://localhost:8080](http://localhost:8080).

---

## Running Tests

### Unit tests

```bash
dotnet test tests/IncidentMonitor.UnitTests/IncidentMonitor.UnitTests.csproj --verbosity normal
```

### Integration tests

Docker must be running — Testcontainers spins up a real PostgreSQL instance automatically.

```bash
dotnet test tests/IncidentMonitor.IntegrationTests/IncidentMonitor.IntegrationTests.csproj --verbosity normal
```

### All tests

```bash
dotnet test
```

---

## API Reference

Base URL: `http://localhost:5153`

### Ingest logs

```
POST /api/logs
```

```json
{
  "logs": [
    {
      "serviceName": "payment-service",
      "level": "ERROR",
      "message": "Payment gateway timeout after 30s",
      "timestamp": "2026-04-01T12:00:00Z"
    }
  ]
}
```

Accepts batches of up to 500 log entries. Returns `202 Accepted` with the number of entries queued.

| Field | Type | Values |
|---|---|---|
| `serviceName` | string | max 100 chars |
| `level` | string | `INFO`, `WARN`, `ERROR` |
| `message` | string | max 2000 chars |
| `timestamp` | ISO 8601 | optional, defaults to now |

---

### Dashboard stats

```
GET /api/dashboard/stats
```

Returns totals, per-service error rates, and log volume bucketed into 5-minute intervals over the last hour.

---

### Incidents

```
GET /api/dashboard/incidents
```

Returns all open incidents ordered by last seen. Incidents are created automatically when error patterns are detected and grouped by service + normalized message.

---

### Alerts

```
GET /api/dashboard/alerts?count=50
```

Returns the most recent alerts, newest first. Alerts are triggered when the z-score of the error rate in the current 5-minute window exceeds 2.0 standard deviations above the rolling baseline.

---

### Logs

```
GET /api/dashboard/logs?page=1&pageSize=50&service=payment-service&level=ERROR
```

Paginated log query with optional filtering by service name and log level.

---

## How Anomaly Detection Works

The system uses a **z-score approach** over a sliding window of 12 five-minute buckets (one hour of history).

For each service, on every batch processed:

1. The last hour of logs is fetched from the database
2. Error counts are bucketed into 5-minute intervals
3. The mean and standard deviation of the 11 historical buckets are calculated
4. The current bucket's error count is compared against the baseline
5. If the z-score exceeds 2.0 and there are at least 3 errors, an alert is triggered

This approach adapts to each service's normal traffic pattern — a service that consistently generates 50 errors per minute won't alert, but one that suddenly spikes from 2 to 50 will.

---

## How Incident Clustering Works

When an error log is processed, its message is normalized by stripping dynamic values:

- GUIDs → `{guid}`
- Numbers → `{n}`  
- IP addresses → `{ip}`

This means `"Payment failed for order 12345"` and `"Payment failed for order 67890"` map to the same pattern `"payment failed for order {n}"` and are counted as a single incident rather than two separate ones.

---

## Project Structure

```
IncidentMonitor/
├── src/
│   ├── IncidentMonitor.API/            # ASP.NET Core entry point
│   │   ├── Controllers/                # HTTP endpoints
│   │   ├── Validators/                 # FluentValidation rules
│   │   └── Program.cs
│   │
│   ├── IncidentMonitor.Application/    # Business logic, no framework deps
│   │   ├── DTOs/                       # Request/response shapes
│   │   ├── Interfaces/                 # Repository + service contracts
│   │   └── Services/                   # Anomaly detector, clusterer, dashboard
│   │
│   ├── IncidentMonitor.Domain/         # Pure entities and enums
│   │   ├── Entities/                   # LogEntry, Incident, Alert
│   │   └── Enums/                      # LogLevel
│   │
│   ├── IncidentMonitor.Infrastructure/ # EF Core, repos, messaging
│   │   ├── Messaging/                  # RabbitMQ publisher + connection factory
│   │   └── Persistence/                # AppDbContext, repositories, migrations
│   │
│   ├── IncidentMonitor.Worker/         # Background consumer
│   │   └── Consumers/                  # LogIngestionConsumer
│   │
│   └── IncidentMonitor.Simulator/      # Traffic generator
│       └── Services/                   # Per-service simulators
│
├── tests/
│   ├── IncidentMonitor.UnitTests/      # Anomaly, clustering, processing logic
│   └── IncidentMonitor.IntegrationTests/ # API + real DB via Testcontainers
│
├── frontend/                           # React + TypeScript dashboard
│   └── src/
│       ├── api/                        # Axios API client
│       ├── components/                 # Charts, tables, stat cards
│       ├── hooks/                      # useDashboard polling hook
│       └── types/                      # TypeScript interfaces
│
├── docker/                             # Per-service Dockerfiles
├── docker-compose.yml                  # Full local stack
└── .github/workflows/ci.yml           # GitHub Actions pipeline
```

---

## CI/CD

The GitHub Actions pipeline runs on every push and pull request to `main`:

1. **Backend** — restore, build, unit tests, integration tests
2. **Frontend** — install, type-check, production build
3. **Docker** — builds all three images (API, Worker, Frontend) on pushes to `main` only

Test results are uploaded as artifacts and available in the Actions tab after each run.

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Make your changes and ensure tests pass (`dotnet test`)
4. Commit with a meaningful message (`git commit -m "feat: add X"`)
5. Open a pull request against `main`

---

## License

MIT
# Application Architecture
<img width="1410" height="1286" alt="image" src="https://github.com/user-attachments/assets/e2654e3c-6608-4782-af5b-dab60c495fd0" />
