using System.Text.Json;
using Microsoft.Extensions.Logging;
using PWCompanion.Core.Models;

namespace PWCompanion.Core.Memory;

public interface IOffsetConfigurationProvider
{
    OffsetConfiguration Current { get; }
    OffsetConfiguration Reload();
    string ConfigPath { get; }
}

/// <summary>
/// Loads and hot-reloads offset configuration from JSON.
/// </summary>
public sealed class OffsetConfigurationProvider : IOffsetConfigurationProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly ILogger<OffsetConfigurationProvider> _logger;
    private OffsetConfiguration _current;
    private readonly string _configPath;
    private DateTime _lastWriteTime;

    public OffsetConfigurationProvider(ILogger<OffsetConfigurationProvider> logger, string configPath)
    {
        _logger = logger;
        _configPath = configPath;
        _current = LoadOrDefault();
        _lastWriteTime = GetWriteTime();
    }

    public OffsetConfiguration Current => _current;
    public string ConfigPath => _configPath;

    public OffsetConfiguration Reload()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogWarning("offsets.json not found at {Path}. Using defaults. Good luck.", _configPath);
            return _current;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            _current = JsonSerializer.Deserialize<OffsetConfiguration>(json, JsonOptions)
                       ?? new OffsetConfiguration();
            _lastWriteTime = GetWriteTime();
            _logger.LogInformation("Loaded offset config version {Version} from {Path}.", _current.Version, _configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload offsets.json. Keeping previous config. Classic.");
        }

        return _current;
    }

    public bool HasFileChanged()
    {
        if (!File.Exists(_configPath))
            return false;

        return GetWriteTime() > _lastWriteTime;
    }

    private OffsetConfiguration LoadOrDefault()
    {
        if (!File.Exists(_configPath))
        {
            var defaults = new OffsetConfiguration();
            SaveDefaults(defaults);
            return defaults;
        }

        return Reload();
    }

    private void SaveDefaults(OffsetConfiguration config)
    {
        try
        {
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(_configPath, json);
            _logger.LogInformation("Created default offsets.json at {Path}. Update with real values!", _configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not write default offsets.json.");
        }
    }

    private DateTime GetWriteTime() =>
        File.Exists(_configPath) ? File.GetLastWriteTimeUtc(_configPath) : DateTime.MinValue;
}
