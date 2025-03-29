using IncidentManagement.Domain;
using Xunit;
using System;

namespace IncidentManagement.Domain.Tests
{
    public class IncidentTests
    {
        [Fact]
        public void Create_ShouldInitializeIncidentWithDefaultValues()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var name = "Test Incident";
            var description = "This is a test incident.";

            // Act
            var incident = Incident.Create(incidentId, name, description);

            // Assert
            Assert.Equal(incidentId, incident.Id);
            Assert.Equal(name, incident.Name);
            Assert.Equal(description, incident.Description);
            Assert.Equal(IncidentStatus.Open, incident.Status);
            Assert.Equal(Priority.Low, incident.Priority);
            Assert.Empty(incident.Comments);
            Assert.False(incident.Acknowledged);
            Assert.Single(incident.Changes);
            Assert.IsType<IncidentCreatedEvent>(incident.Changes.First());
        }

        [Fact]
        public void AssignAgent_ShouldAddAgentAssignedEvent()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");
            var agentId = Guid.NewGuid();

            // Act
            incident.AssignAgent(agentId);

            // Assert
            Assert.Equal(agentId, incident.AssignedAgentId);
            Assert.Equal(2, incident.Changes.Count); // IncidentCreatedEvent + AgentAssignedEvent
            Assert.IsType<AgentAssignedEvent>(incident.Changes.Last());
        }

        [Fact]
        public void AssignAgent_ShouldThrowIfAgentAlreadyAssigned()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");
            var agentId = Guid.NewGuid();
            incident.AssignAgent(agentId);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => incident.AssignAgent(Guid.NewGuid()));
            Assert.Equal("Agent already assigned.", exception.Message);
        }

        [Fact]
        public void SetPriority_ShouldAddPrioritySetEvent()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");

            // Act
            incident.SetPriority(Priority.High);

            // Assert
            Assert.Equal(Priority.High, incident.Priority);
            Assert.Equal(2, incident.Changes.Count); // IncidentCreatedEvent + PrioritySetEvent
            Assert.IsType<PrioritySetEvent>(incident.Changes.Last());
        }

        [Fact]
        public void AddComment_ShouldAddCommentAddedEvent()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");
            var comment = "This is a comment.";
            var author = "Author";

            // Act
            incident.AddComment(comment, author);

            // Assert
            Assert.Contains(comment, incident.Comments);
            Assert.Equal(2, incident.Changes.Count); // IncidentCreatedEvent + CommentAddedEvent
            Assert.IsType<CommentAddedEvent>(incident.Changes.Last());
        }

        [Fact]
        public void UpdateStatus_ShouldAddStatusUpdatedEvent()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");

            // Act
            incident.UpdateStatus(IncidentStatus.InProgress);

            // Assert
            Assert.Equal(IncidentStatus.InProgress, incident.Status);
            Assert.Equal(2, incident.Changes.Count); // IncidentCreatedEvent + StatusUpdatedEvent
            Assert.IsType<StatusUpdatedEvent>(incident.Changes.Last());
        }

        [Fact]
        public void Acknowledge_ShouldAddIncidentAcknowledgedEvent()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");

            // Act
            incident.Acknowledge();

            // Assert
            Assert.True(incident.Acknowledged);
            Assert.Equal(2, incident.Changes.Count); // IncidentCreatedEvent + IncidentAcknowledgedEvent
            Assert.IsType<IncidentAcknowledgedEvent>(incident.Changes.Last());
        }

        [Fact]
        public void Close_ShouldAddIncidentClosedEvent_WhenResolvedOrAcknowledged()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");
            incident.UpdateStatus(IncidentStatus.Resolved);

            // Act
            incident.Close();

            // Assert
            Assert.Equal(IncidentStatus.Closed, incident.Status);
            Assert.Equal(3, incident.Changes.Count); // IncidentCreatedEvent + StatusUpdatedEvent + IncidentClosedEvent
            Assert.IsType<IncidentClosedEvent>(incident.Changes.Last());
        }

        [Fact]
        public void Close_ShouldThrowIfNotResolvedOrAcknowledged()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => incident.Close());
            Assert.Equal("Cannot close an unresolved or unacknowledged incident.", exception.Message);
        }

        [Fact]
        public void ClearChanges_ShouldClearAllEvents()
        {
            // Arrange
            var incident = Incident.Create(Guid.NewGuid(), "Test Incident", "Description");
            incident.AssignAgent(Guid.NewGuid());

            // Act
            incident.ClearChanges();

            // Assert
            Assert.Empty(incident.Changes);
        }
    }
}