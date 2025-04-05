namespace IncidentManagement.Application
{
    using IncidentManagement.Domain;
    using System;
    using System.Threading.Tasks;

    // Commands
    public record CreateIncidentCommand(string Name, string Description);
    public record AssignAgentCommand(Guid IncidentId, Guid AgentId);
    public record SetPriorityCommand(Guid IncidentId, Priority Priority);
    public record AddCommentCommand(Guid IncidentId, string Comment, string Author);
    public record UpdateStatusCommand(Guid IncidentId, IncidentStatus Status);
    public record AcknowledgeIncidentCommand(Guid IncidentId);
    public record CloseIncidentCommand(Guid IncidentId);

    public class IncidentApplicationService
    {
        private readonly IIncidentRepository _incidentRepository;

        public IncidentApplicationService(IIncidentRepository incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        public async Task<Guid> Handle(CreateIncidentCommand command)
        {
            var incidentId = Guid.NewGuid();
            var incident = Incident.Create(incidentId, command.Name, command.Description);
            await _incidentRepository.SaveAsync(incident);
            return incidentId;
        }

        public async Task Handle(AssignAgentCommand command)
        {
            var incident = await _incidentRepository.GetByIdAsync(command.IncidentId);
            if (incident == null) throw new Exception("Incident not found"); // Handle appropriately

            incident.AssignAgent(command.AgentId);
            await _incidentRepository.SaveAsync(incident);
        }

        public async Task Handle(SetPriorityCommand command)
        {
            var incident = await _incidentRepository.GetByIdAsync(command.IncidentId);
            if (incident == null) throw new Exception("Incident not found");

            incident.SetPriority(command.Priority);
            await _incidentRepository.SaveAsync(incident);
        }

        public async Task Handle(AddCommentCommand command)
        {
            var incident = await _incidentRepository.GetByIdAsync(command.IncidentId);
            if (incident == null) throw new Exception("Incident not found");

            incident.AddComment(command.Comment, command.Author);
            await _incidentRepository.SaveAsync(incident);
        }

        public async Task Handle(UpdateStatusCommand command)
        {
            var incident = await _incidentRepository.GetByIdAsync(command.IncidentId);
            if (incident == null) throw new Exception("Incident not found");

            incident.UpdateStatus(command.Status);
            await _incidentRepository.SaveAsync(incident);
        }

        public async Task Handle(AcknowledgeIncidentCommand command)
        {
            var incident = await _incidentRepository.GetByIdAsync(command.IncidentId);
            if (incident == null) throw new Exception("Incident not found");

            incident.Acknowledge();
            await _incidentRepository.SaveAsync(incident);
        }

        public async Task Handle(CloseIncidentCommand command)
        {
            var incident = await _incidentRepository.GetByIdAsync(command.IncidentId);
            if (incident == null) throw new Exception("Incident not found");

            incident.Close();
            await _incidentRepository.SaveAsync(incident);
        }
    }
}

namespace IncidentManagement.Application.Validation
{
    using FluentValidation;

    public class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
    {
        public CreateIncidentCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}

