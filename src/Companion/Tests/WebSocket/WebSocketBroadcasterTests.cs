using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PWCompanion.Core.Configuration;
using PWCompanion.Core.Models;
using PWCompanion.Core.WebSocket;

namespace PWCompanion.Tests.WebSocket;

public class WebSocketBroadcasterTests : IAsyncLifetime
{
    private WebSocketBroadcaster? _broadcaster;
    private CancellationTokenSource? _cts;
    private const int TestPort = 17848;

    public async Task InitializeAsync()
    {
        _cts = new CancellationTokenSource();
        var settings = Options.Create(new CompanionSettings
        {
            WebSocket = new WebSocketSettings { Port = TestPort, Host = "127.0.0.1" },
        });

        _broadcaster = new WebSocketBroadcaster(NullLogger<WebSocketBroadcaster>.Instance, settings);
        await _broadcaster.StartAsync(_cts.Token);
        await Task.Delay(100);
    }

    public async Task DisposeAsync()
    {
        _cts?.Cancel();
        if (_broadcaster != null)
            await _broadcaster.StopAsync(CancellationToken.None);
        _cts?.Dispose();
    }

    [Fact]
    public async Task ClientConnects_ReceivesWelcomeMessage()
    {
        using var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri($"ws://127.0.0.1:{TestPort}/"), CancellationToken.None);

        var buffer = new byte[4096];
        var result = await client.ReceiveAsync(buffer, CancellationToken.None);
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("server_status", doc.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        using var http = new HttpClient();
        var json = await http.GetStringAsync($"http://127.0.0.1:{TestPort}/health");
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("ok", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal("pw-companion", doc.RootElement.GetProperty("service").GetString());
    }

    [Fact]
    public async Task BroadcastPlayerUpdate_ClientReceivesMessage()
    {
        using var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri($"ws://127.0.0.1:{TestPort}/"), CancellationToken.None);

        // consume welcome
        var buffer = new byte[4096];
        await client.ReceiveAsync(buffer, CancellationToken.None);

        var state = new PlayerState
        {
            X = 123.45f,
            Y = 10f,
            Z = 678.9f,
            MapId = 1,
            IsValid = true,
        };

        await _broadcaster!.BroadcastPlayerUpdateAsync(state);

        var result = await client.ReceiveAsync(buffer, CancellationToken.None);
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("player_update", doc.RootElement.GetProperty("type").GetString());
        Assert.Equal(123.45f, doc.RootElement.GetProperty("x").GetSingle(), precision: 2);
        Assert.Equal(1, doc.RootElement.GetProperty("mapId").GetInt32());
    }
}
