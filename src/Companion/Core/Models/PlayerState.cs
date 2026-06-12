namespace PWCompanion.Core.Models;

/// <summary>
/// Read-only snapshot of player world state from game memory.
/// </summary>
public sealed record PlayerState
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
    public int MapId { get; init; }
    public float? FacingRadians { get; init; }
    public bool IsValid { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public static PlayerState Invalid => new()
    {
        IsValid = false,
        Timestamp = DateTimeOffset.UtcNow,
    };
}
