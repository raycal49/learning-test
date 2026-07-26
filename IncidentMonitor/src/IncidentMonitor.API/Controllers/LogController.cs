using FluentValidation;
using IncidentMonitor.Application.DTOs;
using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using DomainLogLevel = IncidentMonitor.Domain.Enums.LogLevel;

namespace IncidentMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LogsController : ControllerBase
{
    private readonly IMessagePublisher _publisher;
    private readonly IValidator<IngestLogBatchRequest> _validator;
    private readonly ILogger<LogsController> _logger;

    public LogsController(
        IMessagePublisher publisher,
        IValidator<IngestLogBatchRequest> validator,
        ILogger<LogsController> logger)
    {
        _publisher = publisher;
        _validator = validator;
        _logger    = logger;
    }

    /// <summary>Ingest a batch of log entries.</summary>
    [HttpPost]
    public async Task<IActionResult> IngestBatch(
        [FromBody] IngestLogBatchRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        await _publisher.PublishAsync("logs.ingest", request, ct);

        _logger.LogInformation(
            "Accepted batch of {Count} logs for queuing", request.Logs.Count);

        return Accepted(new { queued = request.Logs.Count });
    }
}