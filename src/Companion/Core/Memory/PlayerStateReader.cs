using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PWCompanion.Core.Configuration;
using PWCompanion.Core.Models;
using PWCompanion.Logging;

namespace PWCompanion.Core.Memory;

/// <summary>
/// High-level player state reader using configurable pointer chains.
/// Supports hot-reload of offset configuration.
/// </summary>
public interface IPlayerStateReader
{
    PlayerState Read();
    void ReloadOffsets();
    OffsetConfiguration CurrentOffsets { get; }
    event EventHandler<OffsetConfiguration>? OffsetsReloaded;
}

public sealed class PlayerStateReader : IPlayerStateReader
{
    private readonly IMemoryReader _memoryReader;
    private readonly IOffsetConfigurationProvider _offsetProvider;
    private readonly ILogger<PlayerStateReader> _logger;
    private readonly CompanionSettings _settings;
    private OffsetConfiguration _offsets;
    private float _lastX;
    private float _lastZ;
    private int _movementLogCounter;

    public PlayerStateReader(
        IMemoryReader memoryReader,
        IOffsetConfigurationProvider offsetProvider,
        IOptions<CompanionSettings> settings,
        ILogger<PlayerStateReader> logger)
    {
        _memoryReader = memoryReader;
        _offsetProvider = offsetProvider;
        _settings = settings.Value;
        _logger = logger;
        _offsets = offsetProvider.Current;
    }

    public OffsetConfiguration CurrentOffsets => _offsets;
    public event EventHandler<OffsetConfiguration>? OffsetsReloaded;

    public void ReloadOffsets()
    {
        _offsets = _offsetProvider.Reload();
        _logger.LogInformation("Offsets hot-reloaded to version {Version}. Fresh chains, same old grind.", _offsets.Version);
        OffsetsReloaded?.Invoke(this, _offsets);
    }

    public PlayerState Read()
    {
        if (!_memoryReader.IsAttached)
            return PlayerState.Invalid;

        var moduleName = GameProcessAttach.ResolveModuleName(_offsets.ModuleName, _memoryReader);
        var moduleBase = _memoryReader.GetModuleBaseAddress(moduleName);
        if (moduleBase == nint.Zero)
        {
            _logger.LogWarning(ButlerLog.OffsetOutdatedHint("ModuleBase"));
            return PlayerState.Invalid;
        }

        if (!_memoryReader.TryResolvePointerChain(
                moduleBase,
                _offsets.PlayerBase.BaseOffset,
                _offsets.PlayerBase.Offsets,
                out var playerAddress))
        {
            _logger.LogDebug("Pointer chain resolution failed. Offsets probably stale.");
            return PlayerState.Invalid;
        }

        if (!_memoryReader.TryReadFloat(playerAddress + _offsets.PositionX.Offset, out var x) ||
            !_memoryReader.TryReadFloat(playerAddress + _offsets.PositionY.Offset, out var y) ||
            !_memoryReader.TryReadFloat(playerAddress + _offsets.PositionZ.Offset, out var z) ||
            !_memoryReader.TryReadInt32(playerAddress + _offsets.MapId.Offset, out var mapId))
        {
            _logger.LogDebug("Failed to read player fields at {Address:X}. See offsets-guide.md.", playerAddress);
            return PlayerState.Invalid;
        }

        float? facing = null;
        if (_memoryReader.TryReadFloat(playerAddress + _offsets.Facing.Offset, out var facingValue))
            facing = facingValue;

        LogMovementIfInteresting(x, z);

        return new PlayerState
        {
            X = x,
            Y = y,
            Z = z,
            MapId = mapId,
            FacingRadians = facing,
            IsValid = true,
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    private void LogMovementIfInteresting(float x, float z)
    {
        if (_settings.Logging?.SarcasmInDebug != true)
            return;

        var dx = x - _lastX;
        var dz = z - _lastZ;
        var distance = Math.Sqrt(dx * dx + dz * dz);

        if (distance > 3f && ++_movementLogCounter % 50 == 0)
        {
            _logger.LogDebug(
                "Player moved {Distance:F1} meters. Riveting.",
                distance);
        }

        _lastX = x;
        _lastZ = z;
    }
}
