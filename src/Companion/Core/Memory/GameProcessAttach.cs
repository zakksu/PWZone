namespace PWCompanion.Core.Memory;

/// <summary>
/// Resolves and attaches to the running PW client process (TCG uses elementclient_64).
/// </summary>
public static class GameProcessAttach
{
    public static readonly string[] DefaultProcessCandidates =
    [
        "elementclient_64",
        "elementclient",
    ];

    public static readonly string[] DefaultModuleCandidates =
    [
        "elementclient_64.exe",
        "elementclient.exe",
    ];

    public static bool TryAttach(IMemoryReader reader, string primaryProcessName, IEnumerable<string>? aliases = null)
    {
        foreach (var name in EnumerateCandidates(primaryProcessName, aliases))
        {
            if (reader.TryAttach(name))
                return true;
        }

        return false;
    }

    public static string ResolveModuleName(string configuredModule, IMemoryReader reader)
    {
        if (reader.IsAttached)
        {
            foreach (var module in DefaultModuleCandidates)
            {
                if (reader.GetModuleBaseAddress(module) != nint.Zero)
                    return module;
            }

            if (reader.GetModuleBaseAddress(configuredModule) != nint.Zero)
                return configuredModule;
        }

        return configuredModule;
    }

    private static IEnumerable<string> EnumerateCandidates(string primary, IEnumerable<string>? aliases)
    {
        yield return primary;

        if (aliases != null)
        {
            foreach (var alias in aliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                    yield return alias;
            }
        }

        foreach (var fallback in DefaultProcessCandidates)
            yield return fallback;
    }
}
