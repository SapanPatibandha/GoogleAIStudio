namespace IncidentManagement.Infrastructure
{
    using IncidentManagement.Domain;
    using Npgsql;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class PostgresIncidentRepository : IIncidentRepository
    {
        private readonly string _connectionString;

        public PostgresIncidentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Incident> GetByIdAsync(Guid incidentId)
        {
            var events = await LoadEventsAsync(incidentId);
            if (events == null || !events.Any())
            {
                return null;
            }

            var incident = new Incident();
            foreach (var @event in events)
            {
                incident.Apply(@event);
            }

            return incident;
        }

        public async Task SaveAsync(Incident incident)
        {
            await AppendEventsAsync(incident.Id, incident.Changes);
            incident.ClearChanges();
        }


        private async Task<List<Event>> LoadEventsAsync(Guid incidentId)
        {
            var events = new List<Event>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT event_type, event_data FROM incident_events WHERE incident_id = @incidentId ORDER BY timestamp ASC";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("incidentId", incidentId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var eventType = reader.GetString(0);
                            var eventDataJson = reader.GetString(1);

                            Event @event = eventType switch
                            {
                                nameof(IncidentCreatedEvent) => JsonSerializer.Deserialize<IncidentCreatedEvent>(eventDataJson),
                                nameof(AgentAssignedEvent) => JsonSerializer.Deserialize<AgentAssignedEvent>(eventDataJson),
                                nameof(PrioritySetEvent) => JsonSerializer.Deserialize<PrioritySetEvent>(eventDataJson),
                                nameof(CommentAddedEvent) => JsonSerializer.Deserialize<CommentAddedEvent>(eventDataJson),
                                nameof(StatusUpdatedEvent) => JsonSerializer.Deserialize<StatusUpdatedEvent>(eventDataJson),
                                nameof(IncidentAcknowledgedEvent) => JsonSerializer.Deserialize<IncidentAcknowledgedEvent>(eventDataJson),
                                nameof(IncidentClosedEvent) => JsonSerializer.Deserialize<IncidentClosedEvent>(eventDataJson),
                                _ => throw new Exception($"Unknown event type: {eventType}") //Or handle unknown events
                            };

                            events.Add(@event);
                        }
                    }
                }
            }

            return events;
        }


        private async Task AppendEventsAsync(Guid incidentId, IReadOnlyCollection<Event> events)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var @event in events)
                {
                    var eventType = @event.GetType().Name;
                    var eventData = JsonSerializer.Serialize(@event);

                    var sql = "INSERT INTO incident_events (incident_id, event_type, event_data, timestamp) VALUES (@incidentId, @eventType, @eventData::jsonb, @timestamp)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("incidentId", incidentId);
                        command.Parameters.AddWithValue("eventType", eventType);
                        command.Parameters.AddWithValue("eventData", eventData);
                        command.Parameters.AddWithValue("timestamp", @event.Timestamp); // or DateTime.UtcNow

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }

    public interface IDatabaseConnection
    {
        NpgsqlConnection GetConnection();
    }

    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }

    public interface IDatabaseQueryExecutor
    {
        Task<IDataReader> ExecuteReaderAsync(string query, params object[] parameters);
        Task<int> ExecuteNonQueryAsync(string query, params object[] parameters);
    }

    public class DatabaseQueryExecutor : IDatabaseQueryExecutor
    {
        private readonly string _connectionString;

        public DatabaseQueryExecutor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDataReader> ExecuteReaderAsync(string query, params object[] parameters)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand(query, connection);
            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue($"@p{i}", parameters[i]);
            }

            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return reader as IDataReader; // Return as IDataReader for abstraction
        }

        public async Task<int> ExecuteNonQueryAsync(string query, params object[] parameters)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        command.Parameters.AddWithValue($"@p{i}", parameters[i]);
                    }

                    return await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}