using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PWCompanion.Core.Configuration;
using PWCompanion.Core.Map;
using PWCompanion.Core.Memory;
using PWCompanion.Core.Services;

namespace PWCompanion.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCompanionCore(
        this IServiceCollection services,
        string contentRoot,
        string? offsetsPath = null,
        string? dataDirectory = null)
    {
        offsetsPath ??= Path.Combine(contentRoot, "offsets.json");
        dataDirectory ??= Path.Combine(contentRoot, "data");

        services.AddSingleton<IMemoryReader, ProcessMemoryReader>();
        services.AddSingleton<IOffsetConfigurationProvider>(sp =>
            new OffsetConfigurationProvider(
                sp.GetRequiredService<ILogger<OffsetConfigurationProvider>>(),
                offsetsPath));
        services.AddSingleton<IPlayerStateReader, PlayerStateReader>();
        services.AddSingleton<WebSocket.WebSocketBroadcaster>();
        services.AddSingleton<WebSocket.IWebSocketBroadcaster>(sp =>
            sp.GetRequiredService<WebSocket.WebSocketBroadcaster>());
        services.AddHostedService<CompanionHostedService>();
        services.AddHostedService<Map.StaticMapServer>();

        services.AddSingleton<IGatheringDataService>(sp =>
            new GatheringDataService(
                sp.GetRequiredService<ILogger<GatheringDataService>>(),
                dataDirectory));

        return services;
    }

    public static IServiceCollection AddCompanionSettings(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<CompanionSettings>(configuration.GetSection(CompanionSettings.SectionName));
        return services;
    }
}

public interface IGatheringDataService
{
    MapsCatalog GetMapsCatalog();
    GatheringMapData? GetResourcesForMap(int mapId);
}

public sealed class GatheringDataService : IGatheringDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private readonly ILogger<GatheringDataService> _logger;
    private readonly string _dataDirectory;
    private MapsCatalog? _mapsCatalog;

    public GatheringDataService(ILogger<GatheringDataService> logger, string dataDirectory)
    {
        _logger = logger;
        _dataDirectory = Path.GetFullPath(dataDirectory);
    }

    public MapsCatalog GetMapsCatalog()
    {
        if (_mapsCatalog != null)
            return _mapsCatalog;

        var path = Path.Combine(_dataDirectory, "maps.json");
        if (!File.Exists(path))
        {
            _logger.LogWarning("maps.json not found at {Path}.", path);
            return _mapsCatalog = new MapsCatalog();
        }

        var json = File.ReadAllText(path);
        _mapsCatalog = JsonSerializer.Deserialize<MapsCatalog>(json, JsonOptions) ?? new MapsCatalog();
        return _mapsCatalog;
    }

    public GatheringMapData? GetResourcesForMap(int mapId)
    {
        var path = Path.Combine(_dataDirectory, "resources", $"map_{mapId}.json");
        if (!File.Exists(path))
            return null;

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GatheringMapData>(json, JsonOptions);
    }
}

public sealed class GatheringMapData
{
    public int MapId { get; set; }
    public string MapName { get; set; } = string.Empty;
    public List<GatheringNode> Nodes { get; set; } = [];
}

public sealed class GatheringNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Level { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
    public int RespawnSeconds { get; set; }
    public string? Notes { get; set; }
}
