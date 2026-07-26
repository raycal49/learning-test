using Microsoft.Extensions.Logging;

namespace IncidentMonitor.Simulator.Services;

public sealed class OrderServiceSimulator : ServiceSimulatorBase
{
    protected override string ServiceName => "order-service";
    protected override string ApiUrl      => "http://localhost:5153/api/logs";
    protected override int SpikeEveryNBatches => 20;
    protected override int SpikeErrorCount    => 7;

    protected override IReadOnlyList<string> InfoMessages => new[]
    {
        "Order {orderId} created successfully",
        "Order {orderId} shipped via carrier {carrierId}",
        "Inventory reserved for order {orderId}",
        "Order {orderId} delivered successfully",
        "Order {orderId} payment confirmed",
        "Warehouse {warehouseId} fulfilled order {orderId}"
    };

    protected override IReadOnlyList<string> WarnMessages => new[]
    {
        "Order {orderId} processing delayed by {n}ms",
        "Low inventory warning for product {productId}",
        "Carrier API response slow: {n}ms",
        "Order {orderId} requires manual review",
        "Shipping cost calculation retry for order {orderId}"
    };

    protected override IReadOnlyList<string> ErrorMessages => new[]
    {
        "Order {orderId} failed — inventory service unreachable",
        "Failed to reserve stock for order {orderId}",
        "Carrier API returned 500 for shipment {shipmentId}",
        "Order database write failed for order {orderId}",
        "Payment confirmation timeout for order {orderId}",
        "Warehouse management system connection lost",
        "Order rollback failed — manual intervention required"
    };

    public OrderServiceSimulator(HttpClient http, ILogger<OrderServiceSimulator> logger)
        : base(http, logger) { }
}