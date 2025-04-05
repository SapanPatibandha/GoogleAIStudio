using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace IncidentManagement.Api.Tests
{
    public class IncidentControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IncidentControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateIncident_WithValidData_ReturnsCreated()
        {
            // Arrange
            var incident = new { name = "Test Incident", description = "Test Description" };
            var content = new StringContent(JsonSerializer.Serialize(incident), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/incidents", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task GetIncident_WithValidId_ReturnsIncident()
        {
            // Arrange
            var incidentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/incidents/{incidentId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AssignAgent_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var content = new StringContent(JsonSerializer.Serialize(new { agentId }), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/incidents/{incidentId}/assign-agent", content);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}