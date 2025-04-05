using IncidentManagement.Domain;
using IncidentManagement.Infrastructure;
using IncidentManagement.ReadModels;
using Moq;
using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace IncidentManagement.ReadModels.Tests
{
    public class PostgresIncidentReadModelRepositoryTests
    {
        private readonly Mock<IDatabaseConnection> _databaseConnectionMock;
        private readonly Mock<IDatabaseQueryExecutor> _databaseQueryExecutorMock;
        // private readonly Mock<NpgsqlCommand> _commandMock;
        private readonly PostgresIncidentReadModelRepository _repository;

        public PostgresIncidentReadModelRepositoryTests()
        {
            _databaseConnectionMock = new Mock<IDatabaseConnection>();
            _databaseQueryExecutorMock = new Mock<IDatabaseQueryExecutor>();
            // _commandMock = new Mock<NpgsqlCommand>();

            // var mockConnection = new Mock<NpgsqlConnection>();
            // _databaseConnectionMock.Setup(db => db.GetConnection()).Returns(mockConnection.Object);

            _repository = new PostgresIncidentReadModelRepository(_databaseQueryExecutorMock.Object, _databaseConnectionMock.Object);
        }

        [Fact]
        public async Task GetIncidentReadModelAsync_ShouldReturnReadModel_WhenIncidentExists()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var mockReader = new Mock<IDataReader>();

            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);

            mockReader.Setup(r => r.GetGuid(0)).Returns(incidentId);
            mockReader.Setup(r => r.GetString(1)).Returns("Test Incident");
            mockReader.Setup(r => r.GetString(2)).Returns("Test Description");
            mockReader.Setup(r => r.IsDBNull(3)).Returns(true); // No assigned agent
            mockReader.Setup(r => r.GetString(4)).Returns(Priority.Low.ToString());
            mockReader.Setup(r => r.GetString(5)).Returns(IncidentStatus.Open.ToString());
            mockReader.Setup(r => r.IsDBNull(6)).Returns(true); // No last comment

            _databaseQueryExecutorMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(mockReader.Object);

            // _connectionMock.Setup(c => c.CreateCommand())
            //     .Returns(_commandMock.Object);

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
            var mockReader = new Mock<IDataReader>();

            mockReader.Setup(r => r.Read()).Returns(false);

            _databaseQueryExecutorMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(mockReader.Object);

            // _connectionMock.Setup(c => c.CreateCommand())
            //     .Returns(_commandMock.Object);

            // Act
            var result = await _repository.GetIncidentReadModelAsync(incidentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldInsertIncident_WhenIncidentCreatedEvent()
        {
            var mockReader = new Mock<IDataReader>();

            // Arrange
            var @event = new IncidentCreatedEvent(
                Guid.NewGuid(),
                "Test Incident",
                "Test Description",
                DateTime.UtcNow
            );

            _databaseQueryExecutorMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(mockReader.Object);

            // _commandMock.Setup(c => c.ExecuteNonQueryAsync())
            //     .ReturnsAsync(1);

            // _connectionMock.Setup(c => c.CreateCommand())
            //     .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _databaseQueryExecutorMock.Verify(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldUpdateAgent_WhenAgentAssignedEvent()
        {
            var mockReader = new Mock<IDataReader>();

            // Arrange
            var @event = new AgentAssignedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow
            );

            _databaseQueryExecutorMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(mockReader.Object);

            // _commandMock.Setup(c => c.ExecuteNonQueryAsync())
            //     .ReturnsAsync(1);

            // _connectionMock.Setup(c => c.CreateCommand())
            //     .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _databaseQueryExecutorMock.Verify(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldUpdatePriority_WhenPrioritySetEvent()
        {
            var mockReader = new Mock<IDataReader>();

            // Arrange
            var @event = new PrioritySetEvent(
                Guid.NewGuid(),
                Priority.High,
                DateTime.UtcNow
            );

            _databaseQueryExecutorMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(mockReader.Object);

            // _connectionMock.Setup(c => c.CreateCommand())
            //     .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _databaseQueryExecutorMock.Verify(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldUpdateComment_WhenCommentAddedEvent()
        {
            var mockReader = new Mock<IDataReader>();

            // Arrange
            var @event = new CommentAddedEvent(
                Guid.NewGuid(), // IncidentId
                "Test Comment", // Comment
                "Test User",    // User who added the comment
                DateTime.UtcNow // Timestamp
            );

            _databaseQueryExecutorMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(mockReader.Object);

            // _commandMock.Setup(c => c.ExecuteNonQueryAsync())
            //     .ReturnsAsync(1);

            // _connectionMock.Setup(c => c.CreateCommand())
            //     .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _databaseQueryExecutorMock.Verify(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldUpdateStatus_WhenStatusUpdatedEvent()
        {
            var mockReader = new Mock<IDataReader>();

            // Arrange
            var @event = new StatusUpdatedEvent(
                Guid.NewGuid(), // IncidentId
                IncidentStatus.Resolved, // Status
                DateTime.UtcNow // Timestamp
            );

            _databaseQueryExecutorMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(mockReader.Object);

            // _connectionMock.Setup(c => c.CreateCommand())
            //     .Returns(_commandMock.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _databaseQueryExecutorMock.Verify(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldHandleIncidentClosedEvent()
        {
            var mockReader = new Mock<IDataReader>();

            // Arrange
            var @event = new IncidentClosedEvent(
                Guid.NewGuid(),
                DateTime.UtcNow
            );

            _databaseQueryExecutorMock.Setup(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                .ReturnsAsync(mockReader.Object);

            // Act
            await _repository.UpdateIncidentReadModelAsync(@event);

            // Assert
            _databaseQueryExecutorMock.Verify(c => c.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateIncidentReadModelAsync_ShouldHandleInvalidEventType()
        {
            // Arrange
            var mockReader = new Mock<IDataReader>();
            var invalidEvent = new Mock<Event>(Guid.NewGuid(), DateTime.UtcNow).Object;

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                _repository.UpdateIncidentReadModelAsync(invalidEvent));
        }
    }
}