namespace PWCompanion.Core.Configuration;

public sealed class CompanionSettings
{
    public const string SectionName = "Companion";

    public WebSocketSettings WebSocket { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public MapSettings Map { get; set; } = new();
    public DebugSettings Debug { get; set; } = new();
    public DemoModeSettings DemoMode { get; set; } = new();
    public string DataDirectory { get; set; } = "data";
    public bool PortableMode { get; set; } = true;
}

public sealed class WebSocketSettings
{
    public int Port { get; set; } = 17847;
    public string Host { get; set; } = "127.0.0.1";
    public int MaxConnections { get; set; } = 10;
}

public sealed class LoggingSettings
{
    public string MinimumLevel { get; set; } = "Information";
    public bool SarcasmInDebug { get; set; } = true;
}

public sealed class MapSettings
{
    public string WebMapUrl { get; set; } = "http://localhost:5173";
    public bool AutoOpenBrowser { get; set; } = true;
}

public sealed class DebugSettings
{
    public bool EnableDebugWindow { get; set; } = true;
    public string DebugHotkey { get; set; } = "F12";
    public bool BroadcastMemoryInspection { get; set; } = false;
}
