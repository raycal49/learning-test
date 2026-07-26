using Microsoft.Extensions.Logging;

namespace IncidentMonitor.Simulator.Services;

public sealed class AuthServiceSimulator : ServiceSimulatorBase
{
    protected override string ServiceName => "auth-service";
    protected override string ApiUrl      => "http://localhost:5153/api/logs";
    protected override int SpikeEveryNBatches => 25;
    protected override int SpikeErrorCount    => 8;

    protected override IReadOnlyList<string> InfoMessages => new[]
    {
        "User {userId} logged in successfully",
        "Token issued for user {userId}",
        "Password reset email sent to user {userId}",
        "Session refreshed for user {userId}",
        "OAuth2 flow completed for provider google",
        "MFA verification passed for user {userId}"
    };

    protected override IReadOnlyList<string> WarnMessages => new[]
    {
        "Failed login attempt {n} for user {userId}",
        "Token expiring soon for user {userId}",
        "Unusual login location detected for user {userId}",
        "Rate limit warning: {n} auth requests in 60s",
        "Legacy auth method used by user {userId}"
    };

    protected override IReadOnlyList<string> ErrorMessages => new[]
    {
        "Token validation failed — signature mismatch",
        "JWT secret rotation failed",
        "Auth database unreachable",
        "Session store connection timeout",
        "MFA provider returned error {n}",
        "Account locked after {n} failed attempts",
        "OAuth provider token exchange failed",
        "Redis cache miss cascade — fallback failed"
    };

    public AuthServiceSimulator(HttpClient http, ILogger<AuthServiceSimulator> logger)
        : base(http, logger) { }
}