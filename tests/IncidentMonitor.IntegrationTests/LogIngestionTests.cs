using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using IncidentMonitor.Application.DTOs;

namespace IncidentMonitor.IntegrationTests;

public sealed class LogIngestionTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public LogIngestionTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostLogs_ValidBatch_Returns202()
    {
        var request = new IngestLogBatchRequest(new[]
        {
            new IngestLogRequest(
                "payment-service", "ERROR",
                "Gateway timeout", DateTimeOffset.UtcNow),
            new IngestLogRequest(
                "auth-service", "INFO",
                "User logged in", DateTimeOffset.UtcNow)
        });

        var response = await _client.PostAsJsonAsync("/api/logs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("queued");
        content.Should().Contain("2");
    }

    [Fact]
    public async Task PostLogs_EmptyBatch_Returns400()
    {
        var request = new IngestLogBatchRequest(Array.Empty<IngestLogRequest>());

        var response = await _client.PostAsJsonAsync("/api/logs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostLogs_InvalidLevel_Returns400()
    {
        var request = new IngestLogBatchRequest(new[]
        {
            new IngestLogRequest(
                "payment-service", "INVALID_LEVEL",
                "Some message", DateTimeOffset.UtcNow)
        });

        var response = await _client.PostAsJsonAsync("/api/logs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostLogs_MissingServiceName_Returns400()
    {
        var request = new IngestLogBatchRequest(new[]
        {
            new IngestLogRequest(
                "", "ERROR",
                "Some message", DateTimeOffset.UtcNow)
        });

        var response = await _client.PostAsJsonAsync("/api/logs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostLogs_BatchExceeds500_Returns400()
    {
        var logs = Enumerable.Range(0, 501)
            .Select(i => new IngestLogRequest(
                "payment-service", "INFO",
                $"Message {i}", DateTimeOffset.UtcNow))
            .ToArray();

        var request = new IngestLogBatchRequest(logs);
        var response = await _client.PostAsJsonAsync("/api/logs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}