using IncidentMonitor.Domain.Entities;
using IncidentMonitor.Domain.Enums;

namespace IncidentMonitor.Application.Services;

public sealed class AnomalyDetectionResult
{
    public bool IsAnomaly       { get; init; }
    public double ZScore        { get; init; }
    public double Mean          { get; init; }
    public double StdDev        { get; init; }
    public int ErrorCount       { get; init; }
    public string ServiceName   { get; init; } = default!;
}

public sealed class AnomalyDetector
{
    // How many minutes back we look for the current window
    private const int WindowMinutes = 5;

    // How many historical windows we use to build the baseline
    private const int BaselineWindowCount = 12;

    // Z-score threshold — 2.0 means 2 standard deviations above mean
    private const double ZScoreThreshold = 2.0;

    // Minimum errors before we bother calculating — avoids false positives on cold start
    private const int MinErrorsForDetection = 3;

    public AnomalyDetectionResult Analyze(string serviceName, IReadOnlyList<LogEntry> recentLogs)
    {
        var now = DateTimeOffset.UtcNow;

        // Split logs into time buckets of WindowMinutes each
        var buckets = BuildBuckets(recentLogs, now);

        // Current window = most recent bucket
        var currentCount = buckets.LastOrDefault();

        // Baseline = all buckets except the current one
        var baseline = buckets.Count > 1
            ? buckets.Take(buckets.Count - 1).ToList()
            : new List<int> { 0 };

        var mean   = baseline.Average();
        var stdDev = CalculateStdDev(baseline, mean);
        var zScore = stdDev < 0.0001
            ? 0
            : (currentCount - mean) / stdDev;

        var isAnomaly = currentCount >= MinErrorsForDetection
                     && zScore >= ZScoreThreshold;

        return new AnomalyDetectionResult
        {
            IsAnomaly   = isAnomaly,
            ZScore      = Math.Round(zScore, 2),
            Mean        = Math.Round(mean, 2),
            StdDev      = Math.Round(stdDev, 2),
            ErrorCount  = currentCount,
            ServiceName = serviceName
        };
    }

   private static List<int> BuildBuckets(IReadOnlyList<LogEntry> logs, DateTimeOffset now)
    {
        var buckets = new List<int>();

        for (var i = BaselineWindowCount - 1; i >= 0; i--)
        {
            var bucketEnd   = now.AddMinutes(-(i * WindowMinutes));
            var bucketStart = bucketEnd.AddMinutes(-WindowMinutes);

            var count = logs.Count(l =>
                l.Level     == LogLevel.Error &&
                l.Timestamp >= bucketStart    &&
                l.Timestamp <  bucketEnd);

            buckets.Add(count);
        }

        return buckets;
    }

    private static double CalculateStdDev(IReadOnlyList<int> values, double mean)
    {
        if (values.Count < 2) return 0;

        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / (values.Count - 1));
    }
}