using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PWCompanion.Core.Configuration;
using PWCompanion.Core.Memory;
using PWCompanion.Core.Models;
using PWCompanion.Core.WebSocket;

namespace PWCompanion.Tests.Integration;

/// <summary>
/// Integration-style test using a mock memory reader to validate the read pipeline.
/// </summary>
public class MockMemoryReaderPipelineTests
{
    private sealed class MockMemoryReader : IMemoryReader
    {
        private nint _fakeBase = (nint)0x10000000;
        public bool IsAttached { get; private set; } = true;
        public int? ProcessId => 4242;
        public string ProcessName { get; private set; } = "elementclient";

        public bool TryAttach(string processName)
        {
            ProcessName = processName;
            IsAttached = true;
            return true;
        }

        public void Detach() => IsAttached = false;

        public bool TryReadInt32(nint address, out int value)
        {
            value = 1; // map id
            return true;
        }

        public bool TryReadFloat(nint address, out float value)
        {
            value = address switch
            {
                var a when a == _fakeBase + 0 => 512f,
                var a when a == _fakeBase + 4 => 10f,
                var a when a == _fakeBase + 8 => 768f,
                _ => 0f,
            };
            return true;
        }

        public bool TryReadPointer(nint address, out nint pointer)
        {
            pointer = _fakeBase;
            return true;
        }

        public bool TryResolvePointerChain(nint moduleBase, int baseOffset, int[] offsets, out nint resolvedAddress)
        {
            resolvedAddress = _fakeBase;
            return true;
        }

        public nint GetModuleBaseAddress(string moduleName) => (nint)0x00400000;
        public void Dispose() { }
    }

    [Fact]
    public async Task Broadcaster_SendsPlayerUpdate_FromMockedState()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "pw-mock-" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        var offsetsPath = Path.Combine(tempDir, "offsets.json");
        await File.WriteAllTextAsync(offsetsPath, """
            {
              "Version": "mock",
              "ProcessName": "elementclient",
              "PlayerBase": { "BaseOffset": 0, "Offsets": [] },
              "PositionX": { "Offset": 0 },
              "PositionY": { "Offset": 4 },
              "PositionZ": { "Offset": 8 },
              "MapId": { "Offset": 16 },
              "Facing": { "Offset": 20, "Optional": true }
            }
            """);

        try
        {
            var offsetProvider = new OffsetConfigurationProvider(
                NullLogger<OffsetConfigurationProvider>.Instance,
                offsetsPath);
            offsetProvider.Reload();

            var mockMemory = new MockMemoryReader();
            var settings = Options.Create(new CompanionSettings
            {
                WebSocket = new WebSocketSettings { Port = 17849, Host = "127.0.0.1" },
            });

            var playerReader = new PlayerStateReader(
                mockMemory,
                offsetProvider,
                settings,
                NullLogger<PlayerStateReader>.Instance);

            var broadcaster = new WebSocketBroadcaster(
                NullLogger<WebSocketBroadcaster>.Instance,
                settings);

            using var cts = new CancellationTokenSource();
            await broadcaster.StartAsync(cts.Token);
            await Task.Delay(100, cts.Token);

            using var client = new System.Net.WebSockets.ClientWebSocket();
            await client.ConnectAsync(new Uri("ws://127.0.0.1:17849/"), cts.Token);

            var buffer = new byte[4096];
            await client.ReceiveAsync(buffer, cts.Token); // welcome

            var state = playerReader.Read();
            Assert.True(state.IsValid);
            Assert.Equal(512f, state.X, precision: 1);

            await broadcaster.BroadcastPlayerUpdateAsync(state, cts.Token);

            var result = await client.ReceiveAsync(buffer, cts.Token);
            var json = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
            Assert.Contains("player_update", json);
            Assert.Contains("512", json);

            cts.Cancel();
            await broadcaster.StopAsync(CancellationToken.None);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
