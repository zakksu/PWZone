using Serilog;
using Serilog.Events;

namespace PWCompanion.Logging;

/// <summary>
/// Central Serilog bootstrap with Filipe's Sarcastic PW Butler personality.
/// </summary>
public static class ButlerLog
{
    private static readonly string[] DebugQuips =
    [
        "Another log entry. The excitement is palpable.",
        "Logging this because someone has to care.",
        "If you're reading debug logs, you're my kind of player.",
        "Still here. Still sarcastic. Still logging.",
    ];

    public static ILogger CreateLogger(string logDirectory, LogEventLevel minimumLevel = LogEventLevel.Information)
    {
        Directory.CreateDirectory(logDirectory);

        var logPath = Path.Combine(logDirectory, "pw-companion-.log");

        return new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Butler", "Filipe's Sarcastic PW Butler")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    public static string RandomDebugQuip()
    {
        return DebugQuips[Random.Shared.Next(DebugQuips.Length)];
    }

    public static string OffsetOutdatedHint(string field) =>
        $"Offset for '{field}' looks wrong. Probably outdated — see docs/offsets-guide.md. The butler can't read minds (yet).";

    public static string ProcessNotFoundHint(string processName) =>
        $"No '{processName}' process found. Launch Perfect World first, then I'll attach. I'm good, not psychic.";

    public static string GracefulDegradationHint(string feature) =>
        $"{feature} unavailable — continuing without it. We'll pretend it's a feature.";
}
