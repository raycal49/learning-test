FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY IncidentMonitor.sln .
COPY src/IncidentMonitor.Domain/               src/IncidentMonitor.Domain/
COPY src/IncidentMonitor.Application/          src/IncidentMonitor.Application/
COPY src/IncidentMonitor.Infrastructure/       src/IncidentMonitor.Infrastructure/
COPY src/IncidentMonitor.Worker/               src/IncidentMonitor.Worker/

RUN dotnet restore src/IncidentMonitor.Worker/IncidentMonitor.Worker.csproj
RUN dotnet publish  src/IncidentMonitor.Worker/IncidentMonitor.Worker.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IncidentMonitor.Worker.dll"]