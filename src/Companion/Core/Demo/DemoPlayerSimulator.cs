using PWCompanion.Core.Models;

namespace PWCompanion.Core.Demo;

/// <summary>
/// Generates fake player positions for demo/offline testing without the game client.
/// </summary>
public sealed class DemoPlayerSimulator
{
    private readonly float _speed;
    private readonly int _mapId;
    private readonly (float X, float Z)[] _waypoints;
    private int _waypointIndex;
    private float _x;
    private float _z;
    private float _y = 10f;
    private bool _initialized;

    public DemoPlayerSimulator(int mapId, float speed, IEnumerable<(float X, float Z)>? waypoints = null)
    {
        _mapId = mapId;
        _speed = speed;
        _waypoints = waypoints?.ToArray() ?? DefaultWaypoints();
        _x = _waypoints[0].X;
        _z = _waypoints[0].Z;
    }

    public PlayerState Tick()
    {
        if (!_initialized)
        {
            _initialized = true;
            return CreateState(facing: 0);
        }

        var target = _waypoints[_waypointIndex];
        var dx = target.X - _x;
        var dz = target.Z - _z;
        var dist = MathF.Sqrt(dx * dx + dz * dz);

        float facing;
        if (dist <= _speed)
        {
            _x = target.X;
            _z = target.Z;
            _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
            facing = MathF.Atan2(
                _waypoints[_waypointIndex].X - _x,
                _waypoints[_waypointIndex].Z - _z);
        }
        else
        {
            _x += dx / dist * _speed;
            _z += dz / dist * _speed;
            facing = MathF.Atan2(dx, dz);
        }

        return CreateState(facing);
    }

    private PlayerState CreateState(float facing) => new()
    {
        X = _x,
        Y = _y,
        Z = _z,
        MapId = _mapId,
        FacingRadians = facing,
        IsValid = true,
        Timestamp = DateTimeOffset.UtcNow,
    };

    /// <summary>
    /// Sample circuit matching data/resources/map_1.json nodes.
    /// </summary>
    private static (float X, float Z)[] DefaultWaypoints() =>
    [
        (512, 768),
        (320, 640),
        (200, 400),
        (600, 500),
        (680, 320),
        (768, 256),
        (512, 768),
    ];
}
