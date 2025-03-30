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

## Prerequisites
- [.NET 9.0 or later](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download)
- [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/)
- [Visual Studio Code](https://code.visualstudio.com/) or any IDE of your choice

## Setup Instructions

### Clone the Repository
```bash
git clone https://github.com/SapanPatibandha/GoogleAIStudio.git
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

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a feature branch.
3. Submit a pull request.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact
For questions or feedback, please contact [your-email@example.com](mailto:your-email@example.com).