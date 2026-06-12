using PWCompanion.Core.Memory;

namespace PWCompanion.Tests.Memory;

public class ProcessMemoryReaderTests
{
    [Fact]
    public void IsAttached_InitiallyFalse()
    {
        using var reader = new ProcessMemoryReader(Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessMemoryReader>.Instance);
        Assert.False(reader.IsAttached);
        Assert.Null(reader.ProcessId);
    }

    [Fact]
    public void TryAttach_NonExistentProcess_ReturnsFalse()
    {
        using var reader = new ProcessMemoryReader(Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessMemoryReader>.Instance);
        var result = reader.TryAttach("this_process_definitely_does_not_exist_pw_companion_test");
        Assert.False(result);
        Assert.False(reader.IsAttached);
    }

    [Fact]
    public void Detach_WhenNotAttached_DoesNotThrow()
    {
        using var reader = new ProcessMemoryReader(Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessMemoryReader>.Instance);
        reader.Detach();
        Assert.False(reader.IsAttached);
    }

    [Fact]
    public void TryReadInt32_WhenNotAttached_ReturnsFalse()
    {
        using var reader = new ProcessMemoryReader(Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessMemoryReader>.Instance);
        var result = reader.TryReadInt32((nint)0x1000, out _);
        Assert.False(result);
    }

    [Fact]
    public void GetModuleBaseAddress_WhenNotAttached_ReturnsZero()
    {
        using var reader = new ProcessMemoryReader(Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessMemoryReader>.Instance);
        var baseAddr = reader.GetModuleBaseAddress("elementclient.exe");
        Assert.Equal(nint.Zero, baseAddr);
    }
}
