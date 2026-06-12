using Microsoft.Extensions.Logging.Abstractions;
using PWCompanion.Core;

namespace PWCompanion.Tests.Data;

public class GatheringDataServiceTests
{
    [Fact]
    public void GetResourcesForMap_LoadsExampleData()
    {
        var dataDir = FindDataDirectory();
        var service = new GatheringDataService(NullLogger<GatheringDataService>.Instance, dataDir);
        var resources = service.GetResourcesForMap(1);

        Assert.NotNull(resources);
        Assert.Equal(1, resources!.MapId);
        Assert.NotEmpty(resources.Nodes);
    }

    [Fact]
    public void GetMapsCatalog_LoadsMapsJson()
    {
        var dataDir = FindDataDirectory();
        var service = new GatheringDataService(NullLogger<GatheringDataService>.Instance, dataDir);
        var catalog = service.GetMapsCatalog();

        Assert.NotEmpty(catalog.Maps);
    }

    [Fact]
    public void GetResourcesForMap_UnknownMap_ReturnsNull()
    {
        var dataDir = FindDataDirectory();
        var service = new GatheringDataService(NullLogger<GatheringDataService>.Instance, dataDir);
        var resources = service.GetResourcesForMap(99999);
        Assert.Null(resources);
    }

    private static string FindDataDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "data");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data"));
    }
}
