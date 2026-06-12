namespace PWCompanion.Core.Models;

/// <summary>
/// WebSocket message broadcast to the web map client.
/// </summary>
public sealed record PlayerUpdateMessage
{
    public string Type { get; init; } = "player_update";
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
    public int MapId { get; init; }
    public float? Facing { get; init; }
    public long Timestamp { get; init; }
    public bool IsValid { get; init; }
    public string? Quip { get; init; }
}

public sealed record ServerStatusMessage
{
    public string Type { get; init; } = "server_status";
    public bool Attached { get; init; }
    public string ProcessName { get; init; } = string.Empty;
    public int? ProcessId { get; init; }
    public string OffsetVersion { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed record DebugMemoryMessage
{
    public string Type { get; init; } = "debug_memory";
    public string Address { get; init; } = string.Empty;
    public string Field { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool Success { get; init; }
}
