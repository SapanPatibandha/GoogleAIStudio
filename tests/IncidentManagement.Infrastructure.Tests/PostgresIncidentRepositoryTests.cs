using IncidentManagement.Domain;
using IncidentManagement.Infrastructure;
using Moq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace IncidentManagement.Infrastructure.Tests
{
    public class PostgresIncidentRepositoryTests
    {
        private readonly Mock<NpgsqlConnection> _connectionMock;
        private readonly Mock<NpgsqlCommand> _commandMock;
        private readonly Mock<NpgsqlDataReader> _readerMock;
        private readonly PostgresIncidentRepository _repository;

        public PostgresIncidentRepositoryTests()
        {
            _connectionMock = new Mock<NpgsqlConnection>();
            _commandMock = new Mock<NpgsqlCommand>();
            _readerMock = new Mock<NpgsqlDataReader>();

            // Mock the connection string
            var connectionString = "Host=localhost;Database=IncidentDB;Username=postgres;Password=postgres";
            _repository = new PostgresIncidentRepository(connectionString);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnIncident_WhenEventsExist()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var events = new List<Event>
            {
                new IncidentCreatedEvent(incidentId, "Test Incident", "Test Description", DateTime.UtcNow),
                new StatusUpdatedEvent(incidentId, IncidentStatus.InProgress, DateTime.UtcNow)
            };

            var serializedEvents = events.ConvertAll(e => JsonSerializer.Serialize(e));
            var eventTypes = new List<string> { nameof(IncidentCreatedEvent), nameof(StatusUpdatedEvent) };

            _readerMock.SetupSequence(r => r.ReadAsync())
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _readerMock.SetupSequence(r => r.GetString(0))
                .Returns(eventTypes[0])
                .Returns(eventTypes[1]);

            _readerMock.SetupSequence(r => r.GetString(1))
                .Returns(serializedEvents[0])
                .Returns(serializedEvents[1]);

            _commandMock.Setup(c => c.ExecuteReaderAsync(default))
                .ReturnsAsync(_readerMock.Object);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            var incident = await _repository.GetByIdAsync(incidentId);

            // Assert
            Assert.NotNull(incident);
            Assert.Equal("Test Incident", incident.Name);
            Assert.Equal("Test Description", incident.Description);
            Assert.Equal(IncidentStatus.InProgress, incident.Status);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNoEventsExist()
        {
            // Arrange
            var incidentId = Guid.NewGuid();

            _readerMock.Setup(r => r.ReadAsync())
                .ReturnsAsync(false);

            _commandMock.Setup(c => c.ExecuteReaderAsync(default))
                .ReturnsAsync(_readerMock.Object);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            var incident = await _repository.GetByIdAsync(incidentId);

            // Assert
            Assert.Null(incident);
        }

        [Fact]
        public async Task SaveAsync_ShouldAppendEventsAndClearChanges()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Test Description");
            incident.UpdateStatus(IncidentStatus.InProgress);

            _commandMock.Setup(c => c.ExecuteNonQueryAsync())
                .ReturnsAsync(1);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            await _repository.SaveAsync(incident);

            // Assert
            Assert.Empty(incident.Changes);
            _commandMock.Verify(c => c.ExecuteNonQueryAsync(), Times.Exactly(incident.Changes.Count));
        }

        [Fact]
        public async Task SaveAsync_ShouldThrowException_WhenDatabaseFails()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Test Description");
            incident.UpdateStatus(IncidentStatus.InProgress);

            _commandMock.Setup(c => c.ExecuteNonQueryAsync())
                .ThrowsAsync(new Exception("Database error"));

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _repository.SaveAsync(incident));
        }
    }
}