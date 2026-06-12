using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PWCompanion.Core.Configuration;
using PWCompanion.Core.Models;

namespace PWCompanion.Core.WebSocket;

public interface IWebSocketBroadcaster
{
    int ClientCount { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task BroadcastPlayerUpdateAsync(PlayerState state, CancellationToken cancellationToken = default);
    Task BroadcastStatusAsync(ServerStatusMessage status, CancellationToken cancellationToken = default);
    Task BroadcastDebugMemoryAsync(DebugMemoryMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Local WebSocket server broadcasting player state to the web map.
/// </summary>
public sealed class WebSocketBroadcaster : IWebSocketBroadcaster
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ILogger<WebSocketBroadcaster> _logger;
    private readonly WebSocketSettings _settings;
    private readonly List<System.Net.WebSockets.WebSocket> _clients = [];
    private readonly object _clientLock = new();
    private HttpListener? _listener;
    private Task? _acceptLoop;

    public WebSocketBroadcaster(
        ILogger<WebSocketBroadcaster> logger,
        IOptions<CompanionSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value.WebSocket;
    }

    public int ClientCount
    {
        get
        {
            lock (_clientLock)
                return _clients.Count;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{_settings.Host}:{_settings.Port}/");
        _listener.Start();

        _logger.LogInformation(
            "WebSocket server listening on ws://{Host}:{Port}/. Map clients may connect.",
            _settings.Host,
            _settings.Port);

        _acceptLoop = AcceptLoopAsync(cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Stop();
        if (_acceptLoop != null)
            await _acceptLoop;

        lock (_clientLock)
        {
            foreach (var client in _clients)
            {
                try { client.Dispose(); } catch { /* ignored */ }
            }

            _clients.Clear();
        }
    }

    public Task BroadcastPlayerUpdateAsync(PlayerState state, CancellationToken cancellationToken = default)
    {
        var message = new PlayerUpdateMessage
        {
            X = state.X,
            Y = state.Y,
            Z = state.Z,
            MapId = state.MapId,
            Facing = state.FacingRadians,
            Timestamp = state.Timestamp.ToUnixTimeMilliseconds(),
            IsValid = state.IsValid,
            Quip = state.IsValid ? null : "Player state invalid. Butler is squinting at memory.",
        };

        return BroadcastJsonAsync(message, cancellationToken);
    }

    public Task BroadcastStatusAsync(ServerStatusMessage status, CancellationToken cancellationToken = default) =>
        BroadcastJsonAsync(status, cancellationToken);

    public Task BroadcastDebugMemoryAsync(DebugMemoryMessage message, CancellationToken cancellationToken = default) =>
        BroadcastJsonAsync(message, cancellationToken);

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener?.IsListening == true)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                if (!context.Request.IsWebSocketRequest)
                {
                    await HandleHttpRequestAsync(context);
                    continue;
                }

                if (ClientCount >= _settings.MaxConnections)
                {
                    context.Response.StatusCode = 503;
                    context.Response.Close();
                    _logger.LogWarning("WebSocket connection rejected — max clients reached.");
                    continue;
                }

                _ = HandleClientAsync(context, cancellationToken);
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
                _logger.LogError(ex, "WebSocket accept loop hiccuped.");
            }
        }
    }

    private async Task HandleHttpRequestAsync(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath.TrimEnd('/') ?? "";
        var response = context.Response;
        response.ContentType = "application/json";

        if (path.Equals("/health", StringComparison.OrdinalIgnoreCase))
        {
            var health = new
            {
                status = "ok",
                service = "pw-companion",
                websocketClients = ClientCount,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                message = "Butler is alive. Barely.",
            };
            var json = JsonSerializer.Serialize(health, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            response.StatusCode = 200;
            await response.OutputStream.WriteAsync(bytes);
            response.Close();
            return;
        }

        response.StatusCode = 404;
        var notFound = JsonSerializer.Serialize(new { error = "Not found. Try /health or WebSocket upgrade." }, JsonOptions);
        var nfBytes = Encoding.UTF8.GetBytes(notFound);
        await response.OutputStream.WriteAsync(nfBytes);
        response.Close();
    }

    private async Task HandleClientAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        System.Net.WebSockets.WebSocket? socket = null;
        try
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            socket = wsContext.WebSocket;

            lock (_clientLock)
                _clients.Add(socket);

            _logger.LogInformation("Map client connected. Total: {Count}. Someone actually opened the map.", ClientCount);

            var welcome = new ServerStatusMessage
            {
                Attached = false,
                Message = "Welcome to PW Companion. The butler is at your service. Mostly.",
            };
            await SendAsync(socket, JsonSerializer.Serialize(welcome, JsonOptions), cancellationToken);

            var buffer = new byte[1024];
            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Client disconnected. They'll be back. They always come back.");
        }
        finally
        {
            if (socket != null)
            {
                lock (_clientLock)
                    _clients.Remove(socket);

                try
                {
                    if (socket.State == WebSocketState.Open)
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
                }
                catch { /* ignored */ }

                socket.Dispose();
            }
        }
    }

    private async Task BroadcastJsonAsync<T>(T message, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(message, JsonOptions);
        List<System.Net.WebSockets.WebSocket> snapshot;

        lock (_clientLock)
            snapshot = [.. _clients];

        foreach (var client in snapshot)
        {
            if (client.State != WebSocketState.Open)
                continue;

            try
            {
                await SendAsync(client, json, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to send to a client. One less audience member.");
            }
        }
    }

    private static Task SendAsync(System.Net.WebSockets.WebSocket socket, string json, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }
}
