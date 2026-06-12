using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PWCompanion.Core.Configuration;

namespace PWCompanion.Core.Map;

/// <summary>
/// Serves the built React map from a local webmap/ folder (no GitHub Pages required).
/// </summary>
public sealed class StaticMapServer : IHostedService
{
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=utf-8",
        [".js"] = "application/javascript; charset=utf-8",
        [".css"] = "text/css; charset=utf-8",
        [".json"] = "application/json; charset=utf-8",
        [".svg"] = "image/svg+xml",
        [".png"] = "image/png",
        [".ico"] = "image/x-icon",
        [".woff2"] = "font/woff2",
        [".map"] = "application/json",
    };

    private readonly ILogger<StaticMapServer> _logger;
    private readonly MapSettings _settings;
    private readonly string _webMapRoot;
    private HttpListener? _listener;
    private Task? _serveLoop;
    private CancellationTokenSource? _cts;

    public StaticMapServer(
        ILogger<StaticMapServer> logger,
        IOptions<CompanionSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value.Map;
        _webMapRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, _settings.EmbeddedMapDirectory));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_settings.ServeEmbeddedMap)
            return Task.CompletedTask;

        if (!Directory.Exists(_webMapRoot) || !File.Exists(Path.Combine(_webMapRoot, "index.html")))
        {
            _logger.LogWarning(
                "Embedded map not found at {Path}. Browser will use {Url} instead.",
                _webMapRoot,
                _settings.WebMapUrl);
            return Task.CompletedTask;
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{_settings.StaticMapPort}/");
        _listener.Start();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _serveLoop = ServeLoopAsync(_cts.Token);

        _logger.LogInformation(
            "Local map server at http://127.0.0.1:{Port}/ (embedded webmap).",
            _settings.StaticMapPort);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        _listener?.Stop();

        if (_serveLoop != null)
        {
            try { await _serveLoop; }
            catch (OperationCanceledException) { /* expected */ }
        }

        _listener?.Close();
    }

    private async Task ServeLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener?.IsListening == true)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context), cancellationToken);
            }
            catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Map server accept error.");
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            var path = context.Request.Url?.AbsolutePath ?? "/";
            var filePath = ResolveFilePath(path);

            if (filePath == null || !File.Exists(filePath))
            {
                var index = Path.Combine(_webMapRoot, "index.html");
                if (File.Exists(index))
                    filePath = index;
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                    return;
                }
            }

            var ext = Path.GetExtension(filePath);
            context.Response.ContentType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");
            context.Response.StatusCode = 200;

            await using var stream = File.OpenRead(filePath);
            await stream.CopyToAsync(context.Response.OutputStream);
            context.Response.Close();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to serve map file.");
            try
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch { /* ignored */ }
        }
    }

    private string? ResolveFilePath(string urlPath)
    {
        var relative = urlPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        if (string.IsNullOrEmpty(relative))
            relative = "index.html";

        var full = Path.GetFullPath(Path.Combine(_webMapRoot, relative));
        if (!full.StartsWith(_webMapRoot, StringComparison.OrdinalIgnoreCase))
            return null;

        return full;
    }
}
