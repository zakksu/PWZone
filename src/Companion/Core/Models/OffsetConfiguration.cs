namespace PWCompanion.Core.Models;

/// <summary>
/// Versioned memory offset configuration for PW 1.8.7 (The Classic Games).
/// Hot-reloadable from offsets.json.
/// </summary>
public sealed class OffsetConfiguration
{
    public string Version { get; set; } = "1.8.7-tcg-placeholder";
    public string ProcessName { get; set; } = "elementclient_64";
    public string ModuleName { get; set; } = "elementclient_64.exe";
    public string[] ProcessNameAliases { get; set; } = ["elementclient"];
    public int PollIntervalMs { get; set; } = 100;

    public PointerChain PlayerBase { get; set; } = new();
    public FieldOffset PositionX { get; set; } = new() { Offset = 0x0, Description = "Player X coordinate" };
    public FieldOffset PositionY { get; set; } = new() { Offset = 0x4, Description = "Player Y coordinate (height)" };
    public FieldOffset PositionZ { get; set; } = new() { Offset = 0x8, Description = "Player Z coordinate" };
    public FieldOffset MapId { get; set; } = new() { Offset = 0x10, Description = "Current world/map ID" };
    public FieldOffset Facing { get; set; } = new() { Offset = 0x14, Description = "Facing direction (radians)", Optional = true };
}

public sealed class PointerChain
{
    public int BaseOffset { get; set; }
    public int[] Offsets { get; set; } = [];
    public string Description { get; set; } = "Player object pointer chain";
}

public sealed class FieldOffset
{
    public int Offset { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Optional { get; set; }
}
