using FluentValidation;
using IncidentMonitor.Application.DTOs;

namespace IncidentMonitor.API.Validators;

public sealed class IngestLogRequestValidator : AbstractValidator<IngestLogRequest>
{
    private static readonly HashSet<string> ValidLevels =
        new(StringComparer.OrdinalIgnoreCase) { "INFO", "WARN", "ERROR" };

    public IngestLogRequestValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Level)
            .NotEmpty()
            .Must(l => ValidLevels.Contains(l))
            .WithMessage("Level must be INFO, WARN, or ERROR.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(2000);
    }
}

public sealed class IngestLogBatchRequestValidator : AbstractValidator<IngestLogBatchRequest>
{
    public IngestLogBatchRequestValidator()
    {
        RuleFor(x => x.Logs)
            .NotEmpty()
            .Must(l => l.Count <= 500)
            .WithMessage("Batch size cannot exceed 500 logs.");

        RuleForEach(x => x.Logs)
            .SetValidator(new IngestLogRequestValidator());
    }
}