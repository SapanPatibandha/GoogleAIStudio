using Xunit;

namespace IncidentManagement.Infrastructure.Tests
{
    public class DatabaseConnectionTests
    {
        [Fact]
        public void GetConnection_ShouldReturnValidConnection()
        {
            // Arrange
            var connectionString = "Host=localhost;Database=TestDB;Username=test;Password=test";
            var databaseConnection = new DatabaseConnection(connectionString);

            // Act
            var connection = databaseConnection.GetConnection();

            // Assert
            Assert.NotNull(connection);
            Assert.Equal(connectionString, connection.ConnectionString);
        }
    }
}