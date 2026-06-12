using PWCompanion.Core.Memory;

namespace PWCompanion.Tests.Memory;

public class GameProcessAttachTests
{
    [Fact]
    public void DefaultCandidates_IncludesTcgProcessName()
    {
        Assert.Contains("elementclient_64", GameProcessAttach.DefaultProcessCandidates);
        Assert.Contains("elementclient", GameProcessAttach.DefaultProcessCandidates);
    }

    [Fact]
    public void DefaultModuleCandidates_IncludesTcgModule()
    {
        Assert.Contains("elementclient_64.exe", GameProcessAttach.DefaultModuleCandidates);
    }

    [Fact]
    public void TryAttach_ReturnsFalseWhenNoProcess()
    {
        using var reader = new ProcessMemoryReader(Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessMemoryReader>.Instance);
        var result = GameProcessAttach.TryAttach(reader, "definitely_not_a_real_pw_process_xyz");
        Assert.False(result);
        Assert.False(reader.IsAttached);
    }
}
