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
            foreach (var (@event, _) in events)
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


        private async Task<List<(Event @event, string eventVersion)>> LoadEventsAsync(Guid incidentId)
        {
            var events = new List<(Event @event, string eventVersion)>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT event_type, event_data, event_version FROM incident_events WHERE incident_id = @incidentId ORDER BY timestamp ASC";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("incidentId", incidentId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var eventType = reader.GetString(0);
                            var eventDataJson = reader.GetString(1);
                            var eventVersion = reader.GetString(2);

                            Event @event = DeserializeEvent(eventType, eventDataJson, eventVersion);

                            events.Add((@event, eventVersion));
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
                    var eventData = JsonSerializer.Serialize(@event, @event.GetType());

                    var sql = "INSERT INTO incident_events (incident_id, event_type, event_data, timestamp, event_version) VALUES (@incidentId, @eventType, @eventData::jsonb, @timestamp, @eventVersion)";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("incidentId", incidentId);
                        command.Parameters.AddWithValue("eventType", eventType);
                        command.Parameters.AddWithValue("eventData", eventData);
                        command.Parameters.AddWithValue("timestamp", @event.Timestamp); // or DateTime.UtcNow
                        command.Parameters.AddWithValue("eventVersion", @event.Version);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        private Event DeserializeEvent(string eventType, string eventDataJson, string eventVersion)
        {
            JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

            return (eventType, eventVersion) switch
            {
                (nameof(IncidentCreatedEvent), "00.01") => JsonSerializer.Deserialize<IncidentCreatedEvent>(eventDataJson, options),
                (nameof(AgentAssignedEvent), "00.01") => JsonSerializer.Deserialize<AgentAssignedEvent>(eventDataJson, options),
                (nameof(PrioritySetEvent), "00.01") => JsonSerializer.Deserialize<PrioritySetEvent>(eventDataJson, options),
                (nameof(CommentAddedEvent), "00.01") => JsonSerializer.Deserialize<CommentAddedEvent>(eventDataJson, options),
                (nameof(StatusUpdatedEvent), "00.01") => JsonSerializer.Deserialize<StatusUpdatedEvent>(eventDataJson, options),
                (nameof(IncidentAcknowledgedEvent), "00.01") => JsonSerializer.Deserialize<IncidentAcknowledgedEvent>(eventDataJson, options),
                (nameof(IncidentClosedEvent), "00.01") => JsonSerializer.Deserialize<IncidentClosedEvent>(eventDataJson, options),
                // ... other events and versions ...
                _ => throw new Exception($"Unknown event type or version: {eventType} - {eventVersion}")
            };
        }

        private IncidentCreatedEvent DeserializeIncidentCreatedEventV2(string eventDataJson, JsonSerializerOptions options)
        {
            var temp = JsonSerializer.Deserialize<TempIncidentCreatedEventV2>(eventDataJson, options);
            return new IncidentCreatedEvent(temp.IncidentId, temp.Name, temp.Description, temp.Timestamp) { Version = "00.02" };
        }

        public class TempIncidentCreatedEventV2
        {
            public Guid IncidentId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime Timestamp { get; set; }
            public string NewField { get; set; } // Example of a new field in v2
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
