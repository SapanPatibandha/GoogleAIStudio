namespace IncidentManagement.ReadModels
{
    using IncidentManagement.Domain;
    using Npgsql;
    using System;
    using System.Threading.Tasks;

    public class IncidentReadModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? AssignedAgentId { get; set; }
        public Priority Priority { get; set; }
        public IncidentStatus Status { get; set; }
        public string LastComment { get; set; }

    }

    public interface IIncidentReadModelRepository
    {
        Task<IncidentReadModel> GetIncidentReadModelAsync(Guid incidentId);
        Task UpdateIncidentReadModelAsync(Event @event); // Consume events to update
    }

    public class PostgresIncidentReadModelRepository : IIncidentReadModelRepository
    {
        private readonly string _connectionString;

        public PostgresIncidentReadModelRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IncidentReadModel> GetIncidentReadModelAsync(Guid incidentId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT id, name, description, assigned_agent_id, priority, status, last_comment FROM incident_read_model WHERE id = @incidentId";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("incidentId", incidentId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new IncidentReadModel
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                AssignedAgentId = reader.IsDBNull(3) ? null : reader.GetGuid(3),
                                Priority = Enum.Parse<Priority>(reader.GetString(4)),
                                Status = Enum.Parse<IncidentStatus>(reader.GetString(5)),
                                LastComment = reader.IsDBNull(6) ? null : reader.GetString(6)
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public async Task UpdateIncidentReadModelAsync(Event @event)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                switch (@event)
                {
                    case IncidentCreatedEvent e:
                        var insertSql = @"
                            INSERT INTO incident_read_model (id, name, description, priority, status)
                            VALUES (@id, @name, @description, @priority, @status)";

                        using (var command = new NpgsqlCommand(insertSql, connection))
                        {
                            command.Parameters.AddWithValue("id", e.IncidentId);
                            command.Parameters.AddWithValue("name", e.Name);
                            command.Parameters.AddWithValue("description", e.Description);
                            command.Parameters.AddWithValue("priority", Priority.Low.ToString()); // Default
                            command.Parameters.AddWithValue("status", IncidentStatus.Open.ToString());
                            await command.ExecuteNonQueryAsync();
                        }
                        break;
                    case AgentAssignedEvent e:
                        var updateAgentSql = "UPDATE incident_read_model SET assigned_agent_id = @agentId WHERE id = @incidentId";
                        using (var command = new NpgsqlCommand(updateAgentSql, connection))
                        {
                            command.Parameters.AddWithValue("incidentId", e.IncidentId);
                            command.Parameters.AddWithValue("agentId", e.AgentId);
                            await command.ExecuteNonQueryAsync();
                        }
                        break;
                    case PrioritySetEvent e:
                        var updatePrioritySql = "UPDATE incident_read_model SET priority = @priority WHERE id = @incidentId";
                        using (var command = new NpgsqlCommand(updatePrioritySql, connection))
                        {
                            command.Parameters.AddWithValue("incidentId", e.IncidentId);
                            command.Parameters.AddWithValue("priority", e.Priority.ToString());
                            await command.ExecuteNonQueryAsync();
                        }
                        break;
                    case CommentAddedEvent e:
                        var updateCommentSql = "UPDATE incident_read_model SET last_comment = @comment WHERE id = @incidentId";
                        using (var command = new NpgsqlCommand(updateCommentSql, connection))
                        {
                            command.Parameters.AddWithValue("incidentId", e.IncidentId);
                            command.Parameters.AddWithValue("comment", e.Comment);
                            await command.ExecuteNonQueryAsync();
                        }
                        break;
                    case StatusUpdatedEvent e:
                        var updateStatusSql = "UPDATE incident_read_model SET status = @status WHERE id = @incidentId";
                        using (var command = new NpgsqlCommand(updateStatusSql, connection))
                        {
                            command.Parameters.AddWithValue("incidentId", e.IncidentId);
                            command.Parameters.AddWithValue("status", e.Status.ToString());
                            await command.ExecuteNonQueryAsync();
                        }
                        break;
                        // Handle other events similarly (Acknowledged, Closed)
                }
            }
        }
    }
}