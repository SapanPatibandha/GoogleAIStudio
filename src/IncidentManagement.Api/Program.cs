using IncidentManagement.Application;
using IncidentManagement.Domain;
using IncidentManagement.Infrastructure;
using IncidentManagement.ReadModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Or read from appsettings.json

builder.Services.AddScoped<IIncidentRepository>(sp => new PostgresIncidentRepository(connectionString));
builder.Services.AddScoped<IIncidentReadModelRepository>(sp =>
{
    var databaseConnection = new DatabaseConnection(connectionString); // Assuming DatabaseConnection implements IDatabaseConnection
    var queryExecutor = sp.GetRequiredService<IDatabaseQueryExecutor>(); // Assuming IDatabaseQueryExecutor is registered
    return new PostgresIncidentReadModelRepository(queryExecutor, databaseConnection);
});
builder.Services.AddScoped<PostgresIncidentRepository>(sp =>
    new PostgresIncidentRepository(connectionString)); // Register PostgresIncidentRepository
builder.Services.AddScoped<IncidentApplicationService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Minimal API Endpoints ---

// Create Incident
app.MapPost("/api/incidents", async (CreateIncidentCommand command, IncidentApplicationService incidentService) =>
{
    var incidentId = await incidentService.Handle(command);
    return Results.Created($"/api/incidents/{incidentId}", incidentId);
})
.WithName("CreateIncident")
.WithOpenApi();

// Get Incident
app.MapGet("/api/incidents/{id}", async (Guid id, IIncidentReadModelRepository readModelRepository) =>
{
    var incident = await readModelRepository.GetIncidentReadModelAsync(id);
    return incident is null ? Results.NotFound() : Results.Ok(incident);
})
.WithName("GetIncident")
.WithOpenApi();

// Assign Agent
app.MapPut("/api/incidents/{id}/assign-agent", async (Guid id, Guid agentId, IncidentApplicationService incidentService) =>
{
    await incidentService.Handle(new AssignAgentCommand(id, agentId));
    return Results.NoContent();
})
.WithName("AssignAgent")
.WithOpenApi();

// Set Priority
app.MapPut("/api/incidents/{id}/set-priority", async (Guid id, Priority priority, IncidentApplicationService incidentService) =>
{
    await incidentService.Handle(new SetPriorityCommand(id, priority));
    return Results.NoContent();
})
.WithName("SetPriority")
.WithOpenApi();

// Add Comment
app.MapPost("/api/incidents/{id}/add-comment", async (Guid id, string comment, string author, IncidentApplicationService incidentService) =>
{
    await incidentService.Handle(new AddCommentCommand(id, comment, author));
    return Results.NoContent();
})
.WithName("AddComment")
.WithOpenApi();

// Update Status
app.MapPut("/api/incidents/{id}/update-status", async (Guid id, IncidentStatus status, IncidentApplicationService incidentService) =>
{
    await incidentService.Handle(new UpdateStatusCommand(id, status));
    return Results.NoContent();
})
.WithName("UpdateStatus")
.WithOpenApi();

// Acknowledge Incident
app.MapPut("/api/incidents/{id}/acknowledge", async (Guid id, IncidentApplicationService incidentService) =>
{
    await incidentService.Handle(new AcknowledgeIncidentCommand(id));
    return Results.NoContent();
})
.WithName("AcknowledgeIncident")
.WithOpenApi();

// Close Incident
app.MapPut("/api/incidents/{id}/close", async (Guid id, IncidentApplicationService incidentService) =>
{
    await incidentService.Handle(new CloseIncidentCommand(id));
    return Results.NoContent();
})
.WithName("CloseIncident")
.WithOpenApi();

// Get Read model Incident
app.MapGet("/api/incidents/{id}/readmodel", async (Guid id, [FromServices] PostgresIncidentRepository readModelRepository) =>
{
    var incident = await readModelRepository.GetByIdAsync(id);
    return incident is null ? Results.NotFound() : Results.Ok(incident);
})
.WithName("GetReadModel")
.WithOpenApi();

// //Rebuild Read Model
// app.MapGet("/api/incidents/{id}/rebuild-readmodel", async (Guid id, PostgresIncidentRepository postgresIncidentRepository) =>
// {
//     // Load all events for the incident
//     var incident = await postgresIncidentRepository.GetByIdAsync(id);
//     return incident is null ? Results.NotFound() : Results.Ok(incident);
// })
// .WithName("RebuildReadModel")
// .WithOpenApi();

app.Run();