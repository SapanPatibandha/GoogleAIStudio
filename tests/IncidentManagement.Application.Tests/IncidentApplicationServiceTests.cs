using IncidentManagement.Application;
using IncidentManagement.Domain;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IncidentManagement.Application.Tests
{
    public class IncidentApplicationServiceTests
    {
        private readonly Mock<IIncidentRepository> _incidentRepositoryMock;
        private readonly IncidentApplicationService _service;

        public IncidentApplicationServiceTests()
        {
            _incidentRepositoryMock = new Mock<IIncidentRepository>();
            _service = new IncidentApplicationService(_incidentRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_CreateIncidentCommand_ShouldCreateAndSaveIncident()
        {
            // Arrange
            var command = new CreateIncidentCommand("Test Incident", "Test Description");

            // Act
            var incidentId = await _service.Handle(command);

            // Assert
            _incidentRepositoryMock.Verify(repo => repo.SaveAsync(It.Is<Incident>(i =>
                i.Name == command.Name &&
                i.Description == command.Description &&
                i.Status == IncidentStatus.Open
            )), Times.Once);

            Assert.NotEqual(Guid.Empty, incidentId);
        }

        [Fact]
        public async Task Handle_AssignAgentCommand_ShouldAssignAgentToIncident()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var command = new AssignAgentCommand(incidentId, agentId);
            var incident = Incident.Create(incidentId, "Test Incident", "Test Description");

            _incidentRepositoryMock.Setup(repo => repo.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act
            await _service.Handle(command);

            // Assert
            _incidentRepositoryMock.Verify(repo => repo.SaveAsync(It.Is<Incident>(i =>
                i.AssignedAgentId == agentId
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_SetPriorityCommand_ShouldSetPriorityForIncident()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var command = new SetPriorityCommand(incidentId, Priority.High);
            var incident = Incident.Create(incidentId, "Test Incident", "Test Description");

            _incidentRepositoryMock.Setup(repo => repo.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act
            await _service.Handle(command);

            // Assert
            _incidentRepositoryMock.Verify(repo => repo.SaveAsync(It.Is<Incident>(i =>
                i.Priority == Priority.High
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_AddCommentCommand_ShouldAddCommentToIncident()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var command = new AddCommentCommand(incidentId, "Test Comment", "Author");
            var incident = Incident.Create(incidentId, "Test Incident", "Test Description");

            _incidentRepositoryMock.Setup(repo => repo.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act
            await _service.Handle(command);

            // Assert
            _incidentRepositoryMock.Verify(repo => repo.SaveAsync(It.Is<Incident>(i =>
                i.Comments.Contains("Test Comment")
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateStatusCommand_ShouldUpdateIncidentStatus()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var command = new UpdateStatusCommand(incidentId, IncidentStatus.InProgress);
            var incident = Incident.Create(incidentId, "Test Incident", "Test Description");

            _incidentRepositoryMock.Setup(repo => repo.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act
            await _service.Handle(command);

            // Assert
            _incidentRepositoryMock.Verify(repo => repo.SaveAsync(It.Is<Incident>(i =>
                i.Status == IncidentStatus.InProgress
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_AcknowledgeIncidentCommand_ShouldAcknowledgeIncident()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var command = new AcknowledgeIncidentCommand(incidentId);
            var incident = Incident.Create(incidentId, "Test Incident", "Test Description");

            _incidentRepositoryMock.Setup(repo => repo.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act
            await _service.Handle(command);

            // Assert
            _incidentRepositoryMock.Verify(repo => repo.SaveAsync(It.Is<Incident>(i =>
                i.Acknowledged
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_CloseIncidentCommand_ShouldCloseIncident()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var command = new CloseIncidentCommand(incidentId);
            var incident = Incident.Create(incidentId, "Test Incident", "Test Description");
            incident.UpdateStatus(IncidentStatus.Resolved);

            _incidentRepositoryMock.Setup(repo => repo.GetByIdAsync(incidentId))
                .ReturnsAsync(incident);

            // Act
            await _service.Handle(command);

            // Assert
            _incidentRepositoryMock.Verify(repo => repo.SaveAsync(It.Is<Incident>(i =>
                i.Status == IncidentStatus.Closed
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_Command_ShouldThrowException_WhenIncidentNotFound()
        {
            // Arrange
            var command = new AssignAgentCommand(Guid.NewGuid(), Guid.NewGuid());

            _incidentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Incident)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.Handle(command));
        }
    }
}