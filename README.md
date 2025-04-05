# Incident Management System

## Overview
The **Incident Management System** is an event-sourced application designed to manage incidents. It follows a clean architecture pattern with separate layers for domain, application, infrastructure, and read models.

## Project Structure
The project is organized as follows:

```
GoogleAIStudio/
├── src/
│   ├── IncidentManagement.Api/          # API layer for exposing endpoints
│   ├── IncidentManagement.Application/  # Application layer for business logic
│   ├── IncidentManagement.Domain/       # Core domain layer with entities and events
│   ├── IncidentManagement.Infrastructure/ # Infrastructure layer for persistence
│   └── IncidentManagement.ReadModels/   # Read models for querying data
├── tests/
│   ├── IncidentManagement.Api.Tests/          # Unit tests for API layer
│   ├── IncidentManagement.Application.Tests/  # Unit tests for Application layer
│   ├── IncidentManagement.Domain.Tests/       # Unit tests for Domain layer
│   ├── IncidentManagement.Infrastructure.Tests/ # Unit tests for Infrastructure layer
│   └── IncidentManagement.ReadModels.Tests/   # Unit tests for Read Models
├── docker/
│   ├── Dockerfile.Api                         # Dockerfile for the API project
│   ├── docker-compose.yml                     # Docker Compose file for orchestrating services
├── GoogleAIStudio.sln                         # Solution file
└── README.md                                  # Project documentation
```

## Features
- **Event Sourcing**: Tracks changes to incidents as a series of events.
- **Clean Architecture**: Separates concerns into distinct layers.
- **PostgreSQL Integration**: Uses PostgreSQL for persistence.
- **Docker Support**: Includes Docker and Docker Compose for containerized deployment.
- **Unit Testing**: Comprehensive test coverage for all layers.
- **RESTful API**: Well-documented REST endpoints for incident management.
- **Async/Await Pattern**: Full asynchronous implementation for better performance.
- **CQRS Pattern**: Separate command and query responsibilities.

## Prerequisites
- [.NET 9.0 or later](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download)
- [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/)
- [Visual Studio Code](https://code.visualstudio.com/) or any IDE of your choice

## Setup Instructions

### Clone the Repository
```bash
git clone https://github.com/SapanPatibandha/IncidentMgmt.CSharp.git
cd GoogleAIStudio
```

### Build and Run with Docker Compose
1. Navigate to the `docker` folder:
   ```bash
   cd docker
   ```

2. Build and start the services:
   ```bash
   docker-compose up --build -d
   ```

3. Access the API at `http://localhost:8080`.

4. Stop and clean up the services:
   ```bash
   docker-compose down
   ```

### Database Setup
The application uses PostgreSQL as the database. Below is the schema for the required tables:

#### **SQL Script**
```sql
-- Table: incident_events
CREATE TABLE IF NOT EXISTS incident_events (
    incident_id UUID NOT NULL,
    event_type VARCHAR(255) NOT NULL,
    event_data JSONB NOT NULL,
    timestamp TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT PK_incident_events PRIMARY KEY (incident_id, timestamp) --can not have duplicate events
);

-- Optionally add an index for faster querying by incident_id
CREATE INDEX IF NOT EXISTS IX_incident_events_incident_id ON incident_events (incident_id);


-- Table: incident_read_model
CREATE TABLE IF NOT EXISTS incident_read_model (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    assigned_agent_id UUID,
    priority VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    last_comment TEXT
);

-- Optionally, add indexes for frequently queried columns
CREATE INDEX IF NOT EXISTS IX_incident_read_model_assigned_agent_id ON incident_read_model (assigned_agent_id);
CREATE INDEX IF NOT EXISTS IX_incident_read_model_status ON incident_read_model (status);
```

### Running Tests
To run all unit tests, execute the following command:
```bash
dotnet test
```

### Generate Code Coverage Report
1. Run tests with code coverage:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

2. Install `ReportGenerator` (if not already installed):
   ```bash
   dotnet tool install -g dotnet-reportgenerator-globaltool
   ```

3. Generate the HTML report:
   ```bash
   reportgenerator -reports:TestResults/**/*.coverage -targetdir:coverage-report -reporttypes:Html
   ```

4. Open the report:
   - Navigate to the `coverage-report` folder.
   - Open `index.html` in your browser.

## API Endpoints

### Incidents
- `GET /api/incidents` - List all incidents
- `GET /api/incidents/{id}` - Get incident details
- `POST /api/incidents` - Create new incident
- `PUT /api/incidents/{id}` - Update incident
- `DELETE /api/incidents/{id}` - Delete incident

### Comments
- `POST /api/incidents/{id}/comments` - Add comment to incident
- `GET /api/incidents/{id}/comments` - Get incident comments

## Environment Variables
The application uses the following environment variables:

```ini
# Database Configuration
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=incidentdb
POSTGRES_USER=your_user
POSTGRES_PASSWORD=your_password

# API Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
```

## Development

### Required Tools
- Visual Studio 2022 or Visual Studio Code
- .NET SDK 9.0
- Docker Desktop
- PostgreSQL Client (optional)

### Visual Studio Code Extensions
- C# Dev Kit
- Docker
- PostgreSQL

### Debugging
1. Set environment variables in `launchSettings.json`
2. Start PostgreSQL using Docker:
   ```bash
   docker-compose up db -d
   ```
3. Run the API project:
   ```bash
   dotnet run --project src/IncidentManagement.Api
   ```

## Troubleshooting

### Common Issues
1. **Error: `column "event_data" is of type jsonb but expression is of type text`**
   - Ensure the `event_data` column in the `Events` table is of type `jsonb`.
   - Update the `AppendEventsAsync` method in the `PostgresIncidentRepository` class to explicitly cast the `eventData` parameter to `jsonb`:
     ```csharp
     var sql = "INSERT INTO incident_events (incident_id, event_type, event_data, timestamp) VALUES (@incidentId, @eventType, @eventData::jsonb, @timestamp)";
     ```

2. **Docker Build Fails with `COPY` Errors**
   - Ensure the `context` in `docker-compose.yml` is set to the root of the project:
     ```yaml
     build:
       context: ..
       dockerfile: docker/Dockerfile
     ```

## Architecture

### Project Dependencies
```
IncidentManagement.Api
└── IncidentManagement.Application
    ├── IncidentManagement.Domain
    └── IncidentManagement.Infrastructure
        └── IncidentManagement.ReadModels
```

### Event Flow
1. API receives command
2. Application layer validates command
3. Domain layer creates/updates entity and generates events
4. Infrastructure layer persists events
5. Read model is updated based on events

## Logging
The application uses Serilog for structured logging with the following sinks:
- Console
- File (rolling daily logs)
- PostgreSQL (for production)

## Monitoring
- Health checks available at `/health`
- Metrics exposed at `/metrics` (Prometheus format)
- OpenTelemetry integration for distributed tracing

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a feature branch.
3. Submit a pull request.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact
For questions or feedback, please contact [your-email@example.com](mailto:your-email@example.com).
