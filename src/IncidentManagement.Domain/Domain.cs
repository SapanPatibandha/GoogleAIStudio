namespace IncidentManagement.Domain
{
    // Base class for all events
    public abstract record Event(Guid IncidentId, DateTime Timestamp, string Version = "00.01");

    // Events
    public record IncidentCreatedEvent(Guid IncidentId, string Name, string Description, DateTime Timestamp, string Version = "00.01") : Event(IncidentId, Timestamp, Version);
    public record AgentAssignedEvent(Guid IncidentId, Guid AgentId, DateTime Timestamp, string Version = "00.01") : Event(IncidentId, Timestamp, Version);
    public record PrioritySetEvent(Guid IncidentId, Priority Priority, DateTime Timestamp, string Version = "00.01") : Event(IncidentId, Timestamp, Version);
    public record CommentAddedEvent(Guid IncidentId, string Comment, string Author, DateTime Timestamp, string Version = "00.01") : Event(IncidentId, Timestamp, Version);
    public record StatusUpdatedEvent(Guid IncidentId, IncidentStatus Status, DateTime Timestamp, string Version = "00.01") : Event(IncidentId, Timestamp, Version);
    public record IncidentAcknowledgedEvent(Guid IncidentId, DateTime Timestamp, string Version = "00.01") : Event(IncidentId, Timestamp, Version);
    public record IncidentClosedEvent(Guid IncidentId, DateTime Timestamp, string Version = "00.01") : Event(IncidentId, Timestamp, Version);

    // Enums
    public enum Priority { Low, Medium, High, Critical }
    public enum IncidentStatus { Open, InProgress, Resolved, Acknowledged, Closed }


    // Aggregate: Incident
    public class Incident
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Guid? AssignedAgentId { get; private set; }
        public Priority Priority { get; private set; }
        public IncidentStatus Status { get; private set; }
        public List<string> Comments { get; private set; } = new List<string>();
        public bool Acknowledged { get; private set; }


        private readonly List<Event> _changes = new();
        public IReadOnlyCollection<Event> Changes => _changes.AsReadOnly();

        // Constructor (private for event sourcing)
        public Incident() { }

        // Factory method
        public static Incident Create(Guid incidentId, string name, string description)
        {
            var incident = new Incident();
            var @event = new IncidentCreatedEvent(incidentId, name, description, DateTime.UtcNow);
            incident.Apply(@event);
            incident._changes.Add(@event); // Add the event to the changes list
            return incident;
        }


        // Apply Event - the heart of event sourcing
        public void Apply(Event @event)
        {
            switch (@event)
            {
                case IncidentCreatedEvent e:
                    Id = e.IncidentId;
                    Name = e.Name;
                    Description = e.Description;
                    Status = IncidentStatus.Open;
                    Priority = Priority.Low; // Default
                    break;
                case AgentAssignedEvent e:
                    AssignedAgentId = e.AgentId;
                    break;
                case PrioritySetEvent e:
                    Priority = e.Priority;
                    break;
                case CommentAddedEvent e:
                    Comments.Add(e.Comment);
                    break;
                case StatusUpdatedEvent e:
                    Status = e.Status;
                    break;
                case IncidentAcknowledgedEvent e:
                    Acknowledged = true;
                    break;
                case IncidentClosedEvent e:
                    Status = IncidentStatus.Closed;
                    break;
            }
        }

        // Business methods (Command Handlers in Application Layer will call these)
        public void AssignAgent(Guid agentId)
        {
            if (AssignedAgentId != null)
            {
                throw new InvalidOperationException("Agent already assigned.");
            }

            var @event = new AgentAssignedEvent(Id, agentId, DateTime.UtcNow);
            Apply(@event);
            _changes.Add(@event);
        }

        public void SetPriority(Priority priority)
        {
            var @event = new PrioritySetEvent(Id, priority, DateTime.UtcNow);
            Apply(@event);
            _changes.Add(@event);
        }

        public void AddComment(string comment, string author)
        {
            var @event = new CommentAddedEvent(Id, comment, author, DateTime.UtcNow);
            Apply(@event);
            _changes.Add(@event);
        }

        public void UpdateStatus(IncidentStatus status)
        {
            var @event = new StatusUpdatedEvent(Id, status, DateTime.UtcNow);
            Apply(@event);
            _changes.Add(@event);
        }

        public void Acknowledge()
        {
            var @event = new IncidentAcknowledgedEvent(Id, DateTime.UtcNow);
            Apply(@event);
            _changes.Add(@event);
        }

        public void Close()
        {
            if (Status != IncidentStatus.Resolved && Status != IncidentStatus.Acknowledged)
            {
                throw new InvalidOperationException("Cannot close an unresolved or unacknowledged incident.");
            }

            var @event = new IncidentClosedEvent(Id, DateTime.UtcNow);
            Apply(@event);
            _changes.Add(@event);
        }

        public void ClearChanges()
        {
            _changes.Clear();
        }

    }


    //Repository Interface
    public interface IIncidentRepository
    {
        Task<Incident> GetByIdAsync(Guid incidentId);
        Task SaveAsync(Incident incident);
    }
}

