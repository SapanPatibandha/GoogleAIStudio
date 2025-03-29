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
├── GoogleAIStudio.sln                         # Solution file
└── README.md                                  # Project documentation
```

## Features
- **Event Sourcing**: Tracks changes to incidents as a series of events.
- **Clean Architecture**: Separates concerns into distinct layers.
- **PostgreSQL Integration**: Uses PostgreSQL for persistence.
- **Unit Testing**: Comprehensive test coverage for all layers.

## Prerequisites
- [.NET 6.0 or later](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download)
- [Visual Studio Code](https://code.visualstudio.com/) or any IDE of your choice

## Setup Instructions

### Clone the Repository
```bash
git clone https://github.com/your-repo/incident-management-system.git
cd incident-management-system
```