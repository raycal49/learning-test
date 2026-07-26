FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY IncidentMonitor.sln .
COPY src/IncidentMonitor.Domain/               src/IncidentMonitor.Domain/
COPY src/IncidentMonitor.Application/          src/IncidentMonitor.Application/
COPY src/IncidentMonitor.Infrastructure/       src/IncidentMonitor.Infrastructure/
COPY src/IncidentMonitor.API/                  src/IncidentMonitor.API/

RUN dotnet restore src/IncidentMonitor.API/IncidentMonitor.API.csproj
RUN dotnet publish  src/IncidentMonitor.API/IncidentMonitor.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "IncidentMonitor.API.dll"]