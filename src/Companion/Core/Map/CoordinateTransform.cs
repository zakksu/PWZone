namespace PWCompanion.Core.Map;

/// <summary>
/// Transforms Perfect World world coordinates to Leaflet map coordinates.
/// PW uses a custom coordinate system per map; transforms are map-specific.
/// </summary>
public static class CoordinateTransform
{
    /// <summary>
    /// Default transform for PW 1.8.7 maps using sMAPtool-style metadata.
    /// mapMeta: originX, originZ, scale (world units per pixel), imageHeight.
    /// </summary>
    public static (double Lat, double Lng) WorldToMap(
        float worldX,
        float worldZ,
        MapTransformMeta meta)
    {
        var pixelX = (worldX - meta.OriginX) / meta.Scale;
        var pixelY = (worldZ - meta.OriginZ) / meta.Scale;

        // Leaflet CRS.Simple: y increases downward; flip for image coords
        var lat = meta.ImageHeight - pixelY;
        var lng = pixelX;

        return (lat, lng);
    }

    public static double Distance2D(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return Math.Sqrt(dx * dx + dz * dz);
    }

    public static double Distance2DMap(
        float worldX1,
        float worldZ1,
        float worldX2,
        float worldZ2,
        MapTransformMeta meta)
    {
        var (lat1, lng1) = WorldToMap(worldX1, worldZ1, meta);
        var (lat2, lng2) = WorldToMap(worldX2, worldZ2, meta);
        var dx = lng2 - lng1;
        var dy = lat2 - lat1;
        return Math.Sqrt(dx * dx + dy * dy) * meta.Scale;
    }
}

public sealed class MapTransformMeta
{
    public int MapId { get; set; }
    public string Name { get; set; } = string.Empty;
    public float OriginX { get; set; }
    public float OriginZ { get; set; }
    public float Scale { get; set; } = 1f;
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public string TileUrl { get; set; } = string.Empty;
}

public sealed class MapsCatalog
{
    public string Version { get; set; } = "1.0.0";
    public List<MapTransformMeta> Maps { get; set; } = [];
}
