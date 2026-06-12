using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PWCompanion.Core.Configuration;
using PWCompanion.Core.Memory;
using PWCompanion.Core.Models;
using PWCompanion.Core.WebSocket;

namespace PWCompanion.UI;

public sealed class DebugWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IMemoryReader _memoryReader;
    private readonly IPlayerStateReader _playerStateReader;
    private readonly IWebSocketBroadcaster _broadcaster;
    private readonly CompanionSettings _settings;
    private readonly ILogger<DebugWindowViewModel> _logger;
    private readonly DispatcherTimer _timer;
    private string _statusText = "Waiting for data...";
    private string _playerText = "—";
    private string _offsetText = "—";
    private string _memoryText = "—";
    private string _logText = string.Empty;

    public DebugWindowViewModel(
        IMemoryReader memoryReader,
        IPlayerStateReader playerStateReader,
        IWebSocketBroadcaster broadcaster,
        IOptions<CompanionSettings> settings,
        ILogger<DebugWindowViewModel> logger)
    {
        _memoryReader = memoryReader;
        _playerStateReader = playerStateReader;
        _broadcaster = broadcaster;
        _settings = settings.Value;
        _logger = logger;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (_, _) => Refresh();
        _timer.Start();
    }

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    public string PlayerText
    {
        get => _playerText;
        private set { _playerText = value; OnPropertyChanged(); }
    }

    public string OffsetText
    {
        get => _offsetText;
        private set { _offsetText = value; OnPropertyChanged(); }
    }

    public string MemoryText
    {
        get => _memoryText;
        private set { _memoryText = value; OnPropertyChanged(); }
    }

    public string LogText
    {
        get => _logText;
        private set { _logText = value; OnPropertyChanged(); }
    }

    public void ReloadOffsets()
    {
        _playerStateReader.ReloadOffsets();
        AppendLog("Offsets reloaded. Fingers crossed.");
    }

    public void InspectMemory()
    {
        if (!_memoryReader.IsAttached)
        {
            AppendLog("Not attached. Launch PW first, hero.");
            return;
        }

        var offsets = _playerStateReader.CurrentOffsets;
        var moduleName = GameProcessAttach.ResolveModuleName(offsets.ModuleName, _memoryReader);
        var moduleBase = _memoryReader.GetModuleBaseAddress(moduleName);
        MemoryText = $"Module: {moduleName}\nModule base: 0x{moduleBase:X}\nPID: {_memoryReader.ProcessId}";

        if (_memoryReader.TryResolvePointerChain(
                moduleBase,
                offsets.PlayerBase.BaseOffset,
                offsets.PlayerBase.Offsets,
                out var playerAddress))
        {
            MemoryText += $"\nPlayer ptr: 0x{playerAddress:X}";
            AppendLog($"Resolved player at 0x{playerAddress:X}. Still read-only, relax.");
        }
        else
        {
            AppendLog("Pointer chain failed. Update offsets.json — see docs/offsets-guide.md.");
        }
    }

    private void Refresh()
    {
        StatusText = _memoryReader.IsAttached
            ? $"Attached to {_memoryReader.ProcessName} (PID {_memoryReader.ProcessId}) | WS clients: {_broadcaster.ClientCount}"
            : "Not attached — waiting for elementclient.exe";

        var offsets = _playerStateReader.CurrentOffsets;
        OffsetText = $"Version: {offsets.Version}\nPoll: {offsets.PollIntervalMs}ms\nConfig: hot-reload enabled";

        var state = _playerStateReader.Read();
        PlayerText = state.IsValid
            ? $"X: {state.X:F2}  Y: {state.Y:F2}  Z: {state.Z:F2}\nMap: {state.MapId}  Facing: {state.FacingRadians?.ToString("F2") ?? "n/a"}"
            : "Invalid — offsets probably outdated";
    }

    private void AppendLog(string message)
    {
        LogText = $"[{DateTime.Now:HH:mm:ss}] {message}\n{LogText}";
        if (LogText.Length > 4000)
            LogText = LogText[..4000];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void Dispose() => _timer.Stop();
}
