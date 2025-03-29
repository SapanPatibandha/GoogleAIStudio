using IncidentManagement.Domain;
using IncidentManagement.ReadModels;
using Moq;
using Npgsql;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IncidentManagement.ReadModels.Tests
{
    public class PostgresIncidentReadModelRepositoryTests
    {
        private readonly Mock<NpgsqlConnection> _connectionMock;
        private readonly Mock<NpgsqlCommand> _commandMock;
        private readonly Mock<NpgsqlDataReader> _readerMock;
        private readonly PostgresIncidentReadModelRepository _repository;

        public PostgresIncidentReadModelRepositoryTests()
        {
            _connectionMock = new Mock<NpgsqlConnection>();
            _commandMock = new Mock<NpgsqlCommand>();
            _readerMock = new Mock<NpgsqlDataReader>();

            var connectionString = "Host=localhost;Database=IncidentDB;Username=postgres;Password=password";
            _repository = new PostgresIncidentReadModelRepository(connectionString);
        }

        [Fact]
        public async Task GetIncidentReadModelAsync_ShouldReturnReadModel_WhenIncidentExists()
        {
            // Arrange
            var incidentId = Guid.NewGuid();

            _readerMock.SetupSequence(r => r.ReadAsync())
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _readerMock.Setup(r => r.GetGuid(0)).Returns(incidentId);
            _readerMock.Setup(r => r.GetString(1)).Returns("Test Incident");
            _readerMock.Setup(r => r.GetString(2)).Returns("Test Description");
            _readerMock.Setup(r => r.IsDBNull(3)).Returns(true); // No assigned agent
            _readerMock.Setup(r => r.GetString(4)).Returns(Priority.Low.ToString());
            _readerMock.Setup(r => r.GetString(5)).Returns(IncidentStatus.Open.ToString());
            _readerMock.Setup(r => r.IsDBNull(6)).Returns(true); // No last comment

            _commandMock.Setup(c => c.ExecuteReaderAsync(default))
                .ReturnsAsync(_readerMock.Object);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            var result = await _repository.GetIncidentReadModelAsync(incidentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(incidentId, result.Id);
            Assert.Equal("Test Incident", result.Name);
            Assert.Equal("Test Description", result.Description);
            Assert.Null(result.AssignedAgentId);
            Assert.Equal(Priority.Low, result.Priority);
            Assert.Equal(IncidentStatus.Open, result.Status);
            Assert.Null(result.LastComment);
        }

        [Fact]
        public async Task GetIncidentReadModelAsync_ShouldReturnNull_WhenIncidentDoesNotExist()
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
            var result = await _repository.GetIncidentReadModelAsync(incidentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldInsertIncident_WhenIncidentCreatedEvent()
        {
            // Arrange
            var @event = new IncidentCreatedEvent(
                Guid.NewGuid(),
                "Test Incident",
                "Test Description",
                DateTime.UtcNow
            );

            _commandMock.Setup(c => c.ExecuteNonQueryAsync())
                .ReturnsAsync(1);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _commandMock.Verify(c => c.ExecuteNonQueryAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldUpdateAgent_WhenAgentAssignedEvent()
        {
            // Arrange
            var @event = new AgentAssignedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow
            );

            _commandMock.Setup(c => c.ExecuteNonQueryAsync())
                .ReturnsAsync(1);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _commandMock.Verify(c => c.ExecuteNonQueryAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldUpdatePriority_WhenPrioritySetEvent()
        {
            // Arrange
            var @event = new PrioritySetEvent(
                Guid.NewGuid(),
                Priority.High,
                DateTime.UtcNow
            );

            _commandMock.Setup(c => c.ExecuteNonQueryAsync())
                .ReturnsAsync(1);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _commandMock.Verify(c => c.ExecuteNonQueryAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldUpdateComment_WhenCommentAddedEvent()
        {
            // Arrange
            var @event = new CommentAddedEvent(
                Guid.NewGuid(), // IncidentId
                "Test Comment", // Comment
                "Test User",    // User who added the comment
                DateTime.UtcNow // Timestamp
            );

            _commandMock.Setup(c => c.ExecuteNonQueryAsync())
                .ReturnsAsync(1);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _commandMock.Verify(c => c.ExecuteNonQueryAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldUpdateStatus_WhenStatusUpdatedEvent()
        {
            // Arrange
            var @event = new StatusUpdatedEvent(
                Guid.NewGuid(), // IncidentId
                IncidentStatus.Resolved, // Status
                DateTime.UtcNow // Timestamp
            );

            _commandMock.Setup(c => c.ExecuteNonQueryAsync())
                .ReturnsAsync(1);

            _connectionMock.Setup(c => c.CreateCommand())
                .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _commandMock.Verify(c => c.ExecuteNonQueryAsync(), Times.Once);
        }
    }
}