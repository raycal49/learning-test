using Microsoft.Extensions.Logging;

namespace IncidentMonitor.Simulator.Services;

public sealed class PaymentServiceSimulator : ServiceSimulatorBase
{
    protected override string ServiceName => "payment-service";
    protected override string ApiUrl      => "http://localhost:5153/api/logs";
    protected override int SpikeEveryNBatches => 15;
    protected override int SpikeErrorCount    => 10;

    protected override IReadOnlyList<string> InfoMessages => new[]
    {
        "Payment processed successfully for order {orderId}",
        "Refund initiated for transaction {txId}",
        "Payment method validated for customer {customerId}",
        "Webhook received from payment provider",
        "Payment session created",
        "3DS authentication completed successfully"
    };

    protected override IReadOnlyList<string> WarnMessages => new[]
    {
        "Payment retry attempt {n} of 3",
        "Slow response from payment gateway: {n}ms",
        "Currency conversion rate stale — refresh pending",
        "Payment provider rate limit approaching",
        "Duplicate payment request detected and deduplicated"
    };

    protected override IReadOnlyList<string> ErrorMessages => new[]
    {
        "Payment gateway timeout after 30s",
        "Payment gateway connection refused",
        "Insufficient funds for transaction",
        "Card declined by issuing bank",
        "Payment provider returned 503",
        "Database connection pool exhausted",
        "Failed to persist payment record",
        "Fraud detection service unreachable"
    };

    public PaymentServiceSimulator(HttpClient http, ILogger<PaymentServiceSimulator> logger)
        : base(http, logger) { }
}