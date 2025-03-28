namespace IncidentManagement.Infrastructure
{
    using IncidentManagement.Domain;
    using Npgsql;
    using System;
    using System.Collections.Generic;
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

                    var sql = "INSERT INTO incident_events (incident_id, event_type, event_data, timestamp) VALUES (@incidentId, @eventType, @eventData, @timestamp)";
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
}