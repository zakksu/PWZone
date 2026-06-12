namespace PWCompanion.Core.Configuration;

public sealed class DemoModeSettings
{
    /// <summary>
    /// When true, simulates player movement instead of reading game memory.
    /// Use while PW is not installed or for UI/pipeline testing.
    /// </summary>
    public bool Enabled { get; set; }

    public int MapId { get; set; } = 1;

    /// <summary>
    /// Movement speed in world units per tick.
    /// </summary>
    public float Speed { get; set; } = 4f;

    /// <summary>
    /// Optional waypoints (X,Z pairs). Empty = circuit through sample gathering nodes.
    /// </summary>
    public float[][] Waypoints { get; set; } = [];
}
