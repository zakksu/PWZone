using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PWCompanion.Core.Memory;

/// <summary>
/// Abstract memory access layer. Implementations must be read-only.
/// </summary>
public interface IMemoryReader : IDisposable
{
    bool IsAttached { get; }
    int? ProcessId { get; }
    string ProcessName { get; }

    bool TryAttach(string processName);
    void Detach();
    bool TryReadInt32(nint address, out int value);
    bool TryReadFloat(nint address, out float value);
    bool TryReadPointer(nint address, out nint pointer);
    bool TryResolvePointerChain(nint moduleBase, int baseOffset, int[] offsets, out nint resolvedAddress);
    nint GetModuleBaseAddress(string moduleName);
}

/// <summary>
/// Win32 process memory reader with safe, bounded reads.
/// Read-only — never writes to game memory.
/// </summary>
public sealed class ProcessMemoryReader : IMemoryReader
{
    private readonly ILogger<ProcessMemoryReader> _logger;
    private nint _processHandle = nint.Zero;
    private int _processId;

    public ProcessMemoryReader(ILogger<ProcessMemoryReader> logger)
    {
        _logger = logger;
    }

    public bool IsAttached => _processHandle != nint.Zero;
    public int? ProcessId => IsAttached ? _processId : null;
    public string ProcessName { get; private set; } = string.Empty;

    public bool TryAttach(string processName)
    {
        Detach();

        var processes = System.Diagnostics.Process.GetProcessesByName(processName);
        if (processes.Length == 0)
        {
            _logger.LogWarning("Process '{ProcessName}' not found. Launch PW first.", processName);
            return false;
        }

        var process = processes[0];
        _processId = process.Id;
        ProcessName = processName;

        _processHandle = OpenProcess(
            ProcessAccessFlags.VmRead | ProcessAccessFlags.QueryInformation,
            false,
            _processId);

        if (_processHandle == nint.Zero)
        {
            _logger.LogError("Failed to open process {ProcessId}. Run as admin? Or anti-cheat said no.", _processId);
            return false;
        }

        _logger.LogInformation(
            "Attached to {ProcessName} (PID {ProcessId}). Player moved 0 meters so far. Riveting.",
            processName,
            _processId);
        return true;
    }

    public void Detach()
    {
        if (_processHandle != nint.Zero)
        {
            CloseHandle(_processHandle);
            _processHandle = nint.Zero;
        }

        _processId = 0;
        ProcessName = string.Empty;
    }

    public bool TryReadInt32(nint address, out int value)
    {
        value = 0;
        Span<byte> buffer = stackalloc byte[4];
        if (!TryReadBytes(address, buffer))
            return false;

        value = BitConverter.ToInt32(buffer);
        return true;
    }

    public bool TryReadFloat(nint address, out float value)
    {
        value = 0;
        Span<byte> buffer = stackalloc byte[4];
        if (!TryReadBytes(address, buffer))
            return false;

        value = BitConverter.ToSingle(buffer);
        return true;
    }

    public bool TryReadPointer(nint address, out nint pointer)
    {
        pointer = nint.Zero;
        if (IntPtr.Size == 8)
        {
            Span<byte> buffer = stackalloc byte[8];
            if (!TryReadBytes(address, buffer))
                return false;
            pointer = (nint)BitConverter.ToInt64(buffer);
        }
        else
        {
            if (!TryReadInt32(address, out var value))
                return false;
            pointer = (nint)value;
        }

        return pointer != nint.Zero;
    }

    public bool TryResolvePointerChain(nint moduleBase, int baseOffset, int[] offsets, out nint resolvedAddress)
    {
        resolvedAddress = nint.Zero;

        if (!IsAttached)
            return false;

        var address = moduleBase + baseOffset;

        for (var i = 0; i < offsets.Length; i++)
        {
            if (!TryReadPointer(address, out var pointer) || pointer == nint.Zero)
                return false;

            address = pointer + offsets[i];
        }

        resolvedAddress = address;
        return true;
    }

    public nint GetModuleBaseAddress(string moduleName)
    {
        if (!IsAttached)
            return nint.Zero;

        try
        {
            var process = System.Diagnostics.Process.GetProcessById(_processId);
            foreach (System.Diagnostics.ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    return module.BaseAddress;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Module enumeration failed for {ModuleName}. Charming.", moduleName);
        }

        return nint.Zero;
    }

    private bool TryReadBytes(nint address, Span<byte> buffer)
    {
        if (!IsAttached || address == nint.Zero)
            return false;

        return ReadProcessMemory(_processHandle, address, buffer, buffer.Length, out _);
    }

    public void Dispose()
    {
        Detach();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint OpenProcess(ProcessAccessFlags access, bool inheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(nint process, nint baseAddress, Span<byte> buffer, int size, out int bytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint handle);

    [Flags]
    private enum ProcessAccessFlags : uint
    {
        QueryInformation = 0x0400,
        VmRead = 0x0010,
    }
}
