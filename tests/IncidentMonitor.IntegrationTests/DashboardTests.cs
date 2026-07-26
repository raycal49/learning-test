using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace IncidentMonitor.IntegrationTests;

public sealed class DashboardTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStats_Returns200_WithCorrectShape()
    {
        var response = await _client.GetAsync("/api/dashboard/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(json).RootElement;

        doc.TryGetProperty("totalLogs",      out _).Should().BeTrue();
        doc.TryGetProperty("totalIncidents", out _).Should().BeTrue();
        doc.TryGetProperty("totalAlerts",    out _).Should().BeTrue();
        doc.TryGetProperty("activeIncidents",out _).Should().BeTrue();
        doc.TryGetProperty("serviceStats",   out _).Should().BeTrue();
        doc.TryGetProperty("volumeOverTime", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetLogs_Returns200_WithPagedShape()
    {
        var response = await _client.GetAsync("/api/dashboard/logs?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(json).RootElement;

        doc.TryGetProperty("items",      out _).Should().BeTrue();
        doc.TryGetProperty("totalCount", out _).Should().BeTrue();
        doc.TryGetProperty("page",       out _).Should().BeTrue();
        doc.TryGetProperty("pageSize",   out _).Should().BeTrue();
        doc.TryGetProperty("totalPages", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetLogs_InvalidPageSize_DefaultsTo50()
    {
        var response = await _client.GetAsync("/api/dashboard/logs?page=1&pageSize=999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(json).RootElement;

        doc.GetProperty("pageSize").GetInt32().Should().Be(50);
    }

    [Fact]
    public async Task GetIncidents_Returns200_WithArray()
    {
        var response = await _client.GetAsync("/api/dashboard/incidents");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().StartWith("[");
    }

    [Fact]
    public async Task GetAlerts_Returns200_WithArray()
    {
        var response = await _client.GetAsync("/api/dashboard/alerts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().StartWith("[");
    }
}