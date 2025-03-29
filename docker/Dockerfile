# Use the official .NET 9.0 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
EXPOSE 8080

# Copy the solution and restore dependencies
COPY ../GoogleAIStudio.sln .
COPY ../src/IncidentManagement.Api/IncidentManagement.Api.csproj src/IncidentManagement.Api/
COPY ../src/IncidentManagement.Application/IncidentManagement.Application.csproj src/IncidentManagement.Application/
COPY ../src/IncidentManagement.Domain/IncidentManagement.Domain.csproj src/IncidentManagement.Domain/
COPY ../src/IncidentManagement.Infrastructure/IncidentManagement.Infrastructure.csproj src/IncidentManagement.Infrastructure/
COPY ../src/IncidentManagement.ReadModels/IncidentManagement.ReadModels.csproj src/IncidentManagement.ReadModels/

RUN dotnet restore "src/IncidentManagement.Api/IncidentManagement.Api.csproj"

# Copy the rest of the application and build it
COPY ../src ./src
WORKDIR "/app/src/IncidentManagement.Api"
RUN dotnet publish -c Release -o /out

# Use the .NET 9.0 runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "IncidentManagement.Api.dll"]