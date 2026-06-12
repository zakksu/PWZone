namespace PWCompanion.Tests.Demo;

using PWCompanion.Core.Demo;

public class DemoPlayerSimulatorTests
{
    [Fact]
    public void Tick_ProducesValidState()
    {
        var sim = new DemoPlayerSimulator(mapId: 1, speed: 4f);
        var state = sim.Tick();
        Assert.True(state.IsValid);
        Assert.Equal(1, state.MapId);
    }

    [Fact]
    public void Tick_AdvancesPositionOverMultipleTicks()
    {
        var sim = new DemoPlayerSimulator(mapId: 1, speed: 50f);
        sim.Tick(); // init
        var start = sim.Tick();
        var later = sim.Tick();

        var moved = Math.Abs(later.X - start.X) + Math.Abs(later.Z - start.Z);
        Assert.True(moved > 0, "Simulator should move the fake player.");
    }

    [Fact]
    public void Tick_CustomWaypoints_StartsAtFirstWaypoint()
    {
        var sim = new DemoPlayerSimulator(
            mapId: 99,
            speed: 1f,
            waypoints: [(100f, 200f), (300f, 400f)]);

        var state = sim.Tick();
        Assert.Equal(100f, state.X, precision: 1);
        Assert.Equal(200f, state.Z, precision: 1);
        Assert.Equal(99, state.MapId);
    }
}
