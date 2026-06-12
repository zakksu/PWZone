using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PWCompanion.Core.Configuration;
using PWCompanion.Core.Demo;
using PWCompanion.Core.Memory;
using PWCompanion.Core.Models;
using PWCompanion.Core.WebSocket;
using PWCompanion.Logging;

namespace PWCompanion.Core.Services;

/// <summary>
/// Background service: attach to PW, poll player state, broadcast via WebSocket.
/// Falls back to demo simulation when DemoMode.Enabled is true.
/// </summary>
public sealed class CompanionHostedService : BackgroundService
{
    private readonly IMemoryReader _memoryReader;
    private readonly IPlayerStateReader _playerStateReader;
    private readonly IWebSocketBroadcaster _broadcaster;
    private readonly IOffsetConfigurationProvider _offsetProvider;
    private readonly CompanionSettings _settings;
    private readonly ILogger<CompanionHostedService> _logger;
    private DemoPlayerSimulator? _demoSimulator;

    public CompanionHostedService(
        IMemoryReader memoryReader,
        IPlayerStateReader playerStateReader,
        IWebSocketBroadcaster broadcaster,
        IOffsetConfigurationProvider offsetProvider,
        IOptions<CompanionSettings> settings,
        ILogger<CompanionHostedService> logger)
    {
        _memoryReader = memoryReader;
        _playerStateReader = playerStateReader;
        _broadcaster = broadcaster;
        _offsetProvider = offsetProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _broadcaster.StartAsync(stoppingToken);

        if (_settings.DemoMode.Enabled)
        {
            _demoSimulator = CreateDemoSimulator();
            _logger.LogInformation(
                "Demo mode active — simulating player on map {MapId}. PW not required. How convenient.",
                _settings.DemoMode.MapId);

            await _broadcaster.BroadcastStatusAsync(new ServerStatusMessage
            {
                Attached = false,
                ProcessName = "demo-mode",
                OffsetVersion = "demo",
                Message = "Demo mode — fake player touring gathering nodes. Game not required.",
            }, stoppingToken);
        }
        else
        {
            _logger.LogInformation("Companion service started. Waiting for Perfect World like a loyal but slightly judgmental butler.");
        }

        var lastAttachAttempt = DateTime.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_settings.DemoMode.Enabled)
                {
                    var demoState = _demoSimulator!.Tick();
                    await _broadcaster.BroadcastPlayerUpdateAsync(demoState, stoppingToken);
                }
                else
                {
                    if (_offsetProvider is OffsetConfigurationProvider provider && provider.HasFileChanged())
                        _playerStateReader.ReloadOffsets();

                    if (!_memoryReader.IsAttached)
                    {
                        if (DateTime.UtcNow - lastAttachAttempt > TimeSpan.FromSeconds(5))
                        {
                            lastAttachAttempt = DateTime.UtcNow;
                            var offsets = _playerStateReader.CurrentOffsets;
                            var attached = GameProcessAttach.TryAttach(
                                _memoryReader,
                                offsets.ProcessName,
                                offsets.ProcessNameAliases);

                            var activeProcess = attached ? _memoryReader.ProcessName : offsets.ProcessName;

                            await _broadcaster.BroadcastStatusAsync(new ServerStatusMessage
                            {
                                Attached = attached,
                                ProcessName = activeProcess,
                                ProcessId = _memoryReader.ProcessId,
                                OffsetVersion = offsets.Version,
                                Message = attached
                                    ? $"Attached to {activeProcess}. Let's find some herbs."
                                    : ButlerLog.ProcessNotFoundHint(offsets.ProcessName),
                            }, stoppingToken);
                        }
                    }
                    else
                    {
                        var state = _playerStateReader.Read();
                        await _broadcaster.BroadcastPlayerUpdateAsync(state, stoppingToken);

                        if (!state.IsValid)
                            _logger.LogDebug("Invalid player state — offsets may need updating.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Poll loop error. Butler needs a moment.");
            }

            var interval = _settings.DemoMode.Enabled
                ? 100
                : _playerStateReader.CurrentOffsets.PollIntervalMs;
            await Task.Delay(interval, stoppingToken);
        }

        await _broadcaster.StopAsync(stoppingToken);
    }

    private DemoPlayerSimulator CreateDemoSimulator()
    {
        var demo = _settings.DemoMode;
        IEnumerable<(float X, float Z)>? waypoints = null;

        if (demo.Waypoints.Length > 0)
        {
            waypoints = demo.Waypoints
                .Where(w => w.Length >= 2)
                .Select(w => (w[0], w[1]));
        }

        return new DemoPlayerSimulator(demo.MapId, demo.Speed, waypoints);
    }
}
