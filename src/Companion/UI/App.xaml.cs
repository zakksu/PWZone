using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PWCompanion.Core;
using PWCompanion.Core.Configuration;
using PWCompanion.Core.Memory;
using PWCompanion.Logging;
using PWCompanion.UI.Views;
using Serilog;

namespace PWCompanion.UI;

public partial class App : Application
{
    private IHost? _host;
    private TaskbarIcon? _trayIcon;
    private DebugWindow? _debugWindow;
    private Window? _hotkeyWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var contentRoot = AppContext.BaseDirectory;
        var logDir = Path.Combine(contentRoot, "logs");

        Log.Logger = ButlerLog.CreateLogger(logDir);

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(contentRoot);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddCompanionSettings(context.Configuration);
                services.AddCompanionCore(contentRoot);
                services.AddSingleton<DebugWindowViewModel>();
            })
            .Build();

        await _host.StartAsync();

        SetupTrayIcon();
        RegisterDebugHotkey();

        var settings = _host.Services.GetRequiredService<IOptions<CompanionSettings>>().Value;
        if (settings.Map.AutoOpenBrowser)
        {
            TryOpenWebMap(settings.Map.WebMapUrl);
        }

        Log.Information("PW Companion started. Filipe's Sarcastic PW Butler reporting for duty.");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _hotkeyWindow?.Close();

        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        await Log.CloseAndFlushAsync();
        base.OnExit(e);
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "PW Companion — Butler Mode",
        };

        var menu = new System.Windows.Controls.ContextMenu();
        menu.Items.Add(CreateMenuItem("Open Map", (_, _) => OpenMap()));
        menu.Items.Add(CreateMenuItem("Debug Window (F12)", (_, _) => ToggleDebugWindow()));
        menu.Items.Add(CreateMenuItem("Reload Offsets", (_, _) => ReloadOffsets()));
        menu.Items.Add(CreateMenuItem("Reattach to PW", (_, _) => Reattach()));
        menu.Items.Add(new System.Windows.Controls.Separator());
        menu.Items.Add(CreateMenuItem("Exit", (_, _) => Shutdown()));

        _trayIcon.ContextMenu = menu;
        _trayIcon.TrayMouseDoubleClick += (_, _) => OpenMap();
    }

    private static System.Windows.Controls.MenuItem CreateMenuItem(string header, RoutedEventHandler handler)
    {
        var item = new System.Windows.Controls.MenuItem { Header = header };
        item.Click += handler;
        return item;
    }

    private void RegisterDebugHotkey()
    {
        var settings = _host!.Services.GetRequiredService<IOptions<CompanionSettings>>().Value;
        if (!settings.Debug.EnableDebugWindow)
            return;

        // Application has no CommandBindings; use a hidden window as the input sink.
        _hotkeyWindow = new Window
        {
            Title = "PWCompanionHotkey",
            Width = 1,
            Height = 1,
            Opacity = 0,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.ToolWindow,
            ResizeMode = ResizeMode.NoResize,
        };

        var gesture = new KeyGesture(Key.F12);
        var command = new RoutedCommand();
        command.InputGestures.Add(gesture);
        _hotkeyWindow.CommandBindings.Add(new CommandBinding(command, (_, _) => ToggleDebugWindow()));
        _hotkeyWindow.Show();
    }

    private void ToggleDebugWindow()
    {
        if (_debugWindow == null || !_debugWindow.IsLoaded)
        {
            _debugWindow = new DebugWindow(_host!.Services.GetRequiredService<DebugWindowViewModel>());
            _debugWindow.Closed += (_, _) => _debugWindow = null;
            _debugWindow.Show();
        }
        else
        {
            _debugWindow.Close();
        }
    }

    private void OpenMap()
    {
        var settings = _host!.Services.GetRequiredService<IOptions<CompanionSettings>>().Value;
        TryOpenWebMap(settings.Map.WebMapUrl);
    }

    private void ReloadOffsets()
    {
        var reader = _host!.Services.GetRequiredService<IPlayerStateReader>();
        reader.ReloadOffsets();
        Log.Information("Offsets reloaded from tray menu. Hope you got them right.");
    }

    private void Reattach()
    {
        var memory = _host!.Services.GetRequiredService<IMemoryReader>();
        memory.Detach();
        Log.Information("Detached. Will reattach on next poll. Butler is patient. Mostly.");
    }

    private static void TryOpenWebMap(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not open browser to {Url}. Open it manually, overachiever.", url);
        }
    }
}
