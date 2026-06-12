using PWCompanion.Core.Map;

namespace PWCompanion.Tests.Map;

public class CoordinateTransformTests
{
    private static MapTransformMeta CreateMeta() => new()
    {
        MapId = 1,
        Name = "Test Map",
        OriginX = 0,
        OriginZ = 0,
        Scale = 2f,
        ImageWidth = 1024,
        ImageHeight = 1024,
    };

    [Fact]
    public void WorldToMap_Origin_ReturnsExpectedPixelCoords()
    {
        var meta = CreateMeta();
        var (lat, lng) = CoordinateTransform.WorldToMap(0, 0, meta);
        Assert.Equal(1024, lat);
        Assert.Equal(0, lng);
    }

    [Fact]
    public void WorldToMap_PositiveCoords_FlipsYAxis()
    {
        var meta = CreateMeta();
        var (lat, lng) = CoordinateTransform.WorldToMap(100, 200, meta);
        Assert.Equal(1024 - 100, lat, precision: 5);
        Assert.Equal(50, lng, precision: 5);
    }

    [Fact]
    public void Distance2D_SamePoint_ReturnsZero()
    {
        var d = CoordinateTransform.Distance2D(100, 200, 100, 200);
        Assert.Equal(0, d);
    }

    [Fact]
    public void Distance2D_UnitDistance_ReturnsCorrectValue()
    {
        var d = CoordinateTransform.Distance2D(0, 0, 3, 4);
        Assert.Equal(5, d);
    }

    [Theory]
    [InlineData(0, 0, 10, 0, 10)]
    [InlineData(0, 0, 0, 10, 10)]
    public void Distance2DMap_MatchesWorldDistance(float x1, float z1, float x2, float z2, double expected)
    {
        var meta = CreateMeta();
        var d = CoordinateTransform.Distance2DMap(x1, z1, x2, z2, meta);
        Assert.Equal(expected, d, precision: 5);
    }
}
