using Microsoft.Extensions.Logging.Abstractions;
using PWCompanion.Core;
using PWCompanion.Core.Memory;

namespace PWCompanion.Tests.Memory;

public class OffsetConfigurationProviderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configPath;

    public OffsetConfigurationProviderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "pw-companion-test-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _configPath = Path.Combine(_tempDir, "offsets.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Reload_LoadsValidJson()
    {
        File.WriteAllText(_configPath, """
            {
              "Version": "test-1.0",
              "ProcessName": "elementclient",
              "PollIntervalMs": 50
            }
            """);

        var provider = new OffsetConfigurationProvider(NullLogger<OffsetConfigurationProvider>.Instance, _configPath);
        var config = provider.Reload();

        Assert.Equal("test-1.0", config.Version);
        Assert.Equal(50, config.PollIntervalMs);
    }

    [Fact]
    public void Reload_MissingFile_KeepsDefaults()
    {
        var provider = new OffsetConfigurationProvider(NullLogger<OffsetConfigurationProvider>.Instance, _configPath);
        Assert.NotNull(provider.Current);
        Assert.Equal("1.8.7-tcg-placeholder", provider.Current.Version);
    }

    [Fact]
    public void HasFileChanged_DetectsModification()
    {
        File.WriteAllText(_configPath, """{"Version":"v1"}""");
        var provider = new OffsetConfigurationProvider(NullLogger<OffsetConfigurationProvider>.Instance, _configPath);
        provider.Reload();
        Assert.False(provider.HasFileChanged());

        Thread.Sleep(50);
        File.WriteAllText(_configPath, """{"Version":"v2"}""");
        Assert.True(provider.HasFileChanged());
    }
}
