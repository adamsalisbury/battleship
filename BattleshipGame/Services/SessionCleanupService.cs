namespace BattleshipGame.Services;

/// <summary>
/// Background service that periodically purges expired game sessions
/// to prevent unbounded memory growth.
/// </summary>
public class SessionCleanupService : BackgroundService
{
    private readonly GameSessionService _sessions;
    private readonly ILogger<SessionCleanupService> _logger;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(30);

    public SessionCleanupService(GameSessionService sessions, ILogger<SessionCleanupService> logger)
    {
        _sessions = sessions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CleanupInterval, stoppingToken);
            _sessions.PurgeExpiredSessions();
            _logger.LogDebug("Session cleanup completed.");
        }
    }
}
