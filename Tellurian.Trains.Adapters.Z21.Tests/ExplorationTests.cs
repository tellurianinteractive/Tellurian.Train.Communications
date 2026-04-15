using Microsoft.Extensions.Logging;
using System.Net;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Communications.Interfaces.Accessories;

namespace Tellurian.Trains.Adapters.Z21.Tests;

/// <summary>
/// Diagnostic tests that talk to a real Z21 to determine how it broadcasts accessory
/// state changes in practice. Run each test individually while following its on-screen
/// instructions (switch a point from the Z21 App at the right moment, etc.) and inspect
/// the test output. None of these assert hard conditions — they log everything and
/// finish with <c>Assert.Inconclusive</c> so the raw evidence shows up in the test runner.
///
/// Z21 endpoint is read from the <c>Z21_ADDRESS</c> environment variable (defaults to
/// 192.168.0.111). Local port 31107 is used to avoid colliding with a running yard app
/// on 21106.
/// </summary>
[Ignore("Z21 must be connected for these tests — comment out this attribute to run locally.")]
[TestCategory("Exploration")]
[TestClass]
public class ExplorationTests
{
    private const int LocalPort = 31107;
    private const int Z21CommandPort = 21105;
    private const short TestTurnoutAddress = 802;

    public required TestContext TestContext { get; set; }

    private static IPEndPoint Z21Endpoint => new(
        IPAddress.Parse(Environment.GetEnvironmentVariable("Z21_ADDRESS") ?? "192.168.0.111"),
        Z21CommandPort);

    private TestContextLogger<T> NewLogger<T>() => new(TestContext);

    /// <summary>
    /// Sanity check: send LAN_GET_SERIAL_NUMBER (no subscription, no accessory). Z21 must reply
    /// with its serial number per spec. If this receives nothing, the Z21 is not talking to us
    /// at all — subscription configuration is a red herring, it's a channel/firewall issue.
    /// </summary>
    [TestMethod]
    public async Task CanTalkToZ21AtAll_GetSerialNumber()
    {
        using var channel = new UdpDataChannel(LocalPort, Z21Endpoint, NewLogger<UdpDataChannel>());
        using var adapter = new Adapter(channel, NewLogger<Adapter>());
        var observer = new LoggingNotificationObserver(TestContext);
        using var _ = adapter.Subscribe(observer);

        await adapter.StartReceiveAsync(TestContext.CancellationToken);
        await Task.Delay(250, TestContext.CancellationToken);

        await adapter.GetSerialNumberAsync(TestContext.CancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.CancellationToken);

        TestContext.WriteLine($"Received {observer.Notifications.Count} notification(s). A Z21 that's reachable MUST reply to GetSerialNumber.");
        Assert.Inconclusive("Check the test output.");
    }

    /// <summary>
    /// Subscribe only to RunningAndSwitching. When prompted, switch a point from the Z21 App.
    /// Every frame the Z21 sends us is logged with its full bytes.
    /// </summary>
    [TestMethod]
    public async Task WaitForXpressNetTurnoutBroadcast()
    {
        using var channel = new UdpDataChannel(LocalPort, Z21Endpoint, NewLogger<UdpDataChannel>());
        using var adapter = new Adapter(channel, NewLogger<Adapter>(), BroadcastSubjects.RunningAndSwitching);
        var observer = new LoggingNotificationObserver(TestContext);
        using var _ = adapter.Subscribe(observer);

        await adapter.StartReceiveAsync(TestContext.CancellationToken);
        await Task.Delay(250, TestContext.CancellationToken);

        TestContext.WriteLine($"*** Switch point {TestTurnoutAddress} from the Z21 App or WLANMaus now. You have 20 seconds. ***");
        await Task.Delay(TimeSpan.FromSeconds(20), TestContext.CancellationToken);

        TestContext.WriteLine($"Received {observer.Notifications.Count} notification(s).");
        Assert.Inconclusive("Check the test output for received frames.");
    }

    /// <summary>
    /// Send LAN_X_SET_TURNOUT for address 1 and capture everything the Z21 sends back.
    /// Tells us whether Z21 echoes the command to the sender at all.
    /// </summary>
    [TestMethod]
    public async Task SendLanXSetTurnout_ObserveResponse()
    {
        using var channel = new UdpDataChannel(LocalPort, Z21Endpoint, NewLogger<UdpDataChannel>());
        using var adapter = new Adapter(channel, NewLogger<Adapter>(), BroadcastSubjects.RunningAndSwitching, useLocoNetForAccessories: false);
        var observer = new LoggingNotificationObserver(TestContext);
        using var _ = adapter.Subscribe(observer);

        await adapter.StartReceiveAsync(TestContext.CancellationToken);
        await Task.Delay(250, TestContext.CancellationToken);

        var address = Address.From(TestTurnoutAddress);
        TestContext.WriteLine($"Sending LAN_X_SET_TURNOUT for address {address.Number} (throw)...");
        await adapter.SetThrownAsync(address, activate: true, TestContext.CancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.CancellationToken);
        await adapter.SetThrownAsync(address, activate: false, TestContext.CancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(3), TestContext.CancellationToken);
        TestContext.WriteLine($"Received {observer.Notifications.Count} notification(s).");
        Assert.Inconclusive("Check the test output for received frames.");
    }

    /// <summary>
    /// Ask the Z21 what broadcast flags are currently active via LAN_GET_BROADCASTFLAGS.
    /// Confirms whether our SubscribeNotifications actually took effect.
    /// </summary>
    [TestMethod]
    public async Task QueryActiveBroadcastFlagsAfterSubscribe()
    {
        using var channel = new UdpDataChannel(LocalPort, Z21Endpoint, NewLogger<UdpDataChannel>());
        using var adapter = new Adapter(channel, NewLogger<Adapter>(), BroadcastSubjects.RunningAndSwitching);
        var observer = new LoggingNotificationObserver(TestContext);
        using var _ = adapter.Subscribe(observer);

        await adapter.StartReceiveAsync(TestContext.CancellationToken);
        await Task.Delay(500, TestContext.CancellationToken);

        await adapter.SendAsync(new GetSubscribedNotificationsCommand(), TestContext.CancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.CancellationToken);

        TestContext.WriteLine($"Received {observer.Notifications.Count} notification(s). Look for a BroadcastSubjectsNotification with the flags value.");
        Assert.Inconclusive("Check the test output.");
    }

    /// <summary>
    /// Subscribe, then "prime" turnout 1 with LAN_X_GET_TURNOUT_INFO, then wait for the user
    /// to switch it from the Z21 App. Some Z21 firmware only broadcasts for turnouts that
    /// have been explicitly queried at least once.
    /// </summary>
    [TestMethod]
    public async Task PrimeTurnoutThenWaitForExternalChange()
    {
        using var channel = new UdpDataChannel(LocalPort, Z21Endpoint, NewLogger<UdpDataChannel>());
        using var adapter = new Adapter(channel, NewLogger<Adapter>(), BroadcastSubjects.RunningAndSwitching);
        var observer = new LoggingNotificationObserver(TestContext);
        using var _ = adapter.Subscribe(observer);

        await adapter.StartReceiveAsync(TestContext.CancellationToken);
        await Task.Delay(250, TestContext.CancellationToken);

        // LAN_X_GET_TURNOUT_INFO — XpressNet packet: 0x43, FAdr_MSB, FAdr_LSB, XOR. FAdr is the
        // 0-based wire address, so user address N maps to FAdr = N - 1.
        short wire = (short)(TestTurnoutAddress - 1);
        byte msb = (byte)((wire >> 8) & 0xFF);
        byte lsb = (byte)(wire & 0xFF);
        byte xor = (byte)(0x43 ^ msb ^ lsb);
        TestContext.WriteLine($"Priming turnout {TestTurnoutAddress} with LAN_X_GET_TURNOUT_INFO (FAdr=0x{wire:X4})...");
        await adapter.SendAsync(new RawXbusCommand([0x43, msb, lsb, xor]), TestContext.CancellationToken);
        await Task.Delay(500, TestContext.CancellationToken);

        TestContext.WriteLine($"*** Now switch point {TestTurnoutAddress} from the Z21 App. You have 20 seconds. ***");
        await Task.Delay(TimeSpan.FromSeconds(20), TestContext.CancellationToken);

        TestContext.WriteLine($"Received {observer.Notifications.Count} notification(s).");
        Assert.Inconclusive("Check the test output for received frames.");
    }

    /// <summary>
    /// Subscribe to both RunningAndSwitching and LocoNetTurnouts. User switches from Z21 App.
    /// Tells us whether the Z21 mirrors XpressNet accessory traffic onto LAN_LOCONET_Z21_RX.
    /// </summary>
    [TestMethod]
    public async Task WaitForChangeWithBothSubscriptions()
    {
        using var channel = new UdpDataChannel(LocalPort, Z21Endpoint, NewLogger<UdpDataChannel>());
        using var adapter = new Adapter(channel, NewLogger<Adapter>(),
            BroadcastSubjects.RunningAndSwitching | BroadcastSubjects.LocoNetTurnouts);
        var observer = new LoggingNotificationObserver(TestContext);
        using var _ = adapter.Subscribe(observer);

        await adapter.StartReceiveAsync(TestContext.CancellationToken);
        await Task.Delay(250, TestContext.CancellationToken);

        TestContext.WriteLine($"*** Switch point {TestTurnoutAddress} from the Z21 App. You have 20 seconds. ***");
        await Task.Delay(TimeSpan.FromSeconds(20), TestContext.CancellationToken);

        TestContext.WriteLine($"Received {observer.Notifications.Count} notification(s).");
        Assert.Inconclusive("Check the test output for received frames.");
    }
}

/// <summary>
/// Raw Xbus command escape hatch — exploration tests need to hand-build XpressNet packets
/// the library doesn't yet expose as first-class commands (e.g. LAN_X_GET_TURNOUT_INFO,
/// whose X-Header's low nibble doesn't match the library's <c>Data.Length</c> convention).
/// Bypasses <c>XpressNetCommand</c> and emits a Z21 Xbus frame with the given raw bytes
/// (including the XpressNet XOR checksum).
/// </summary>
internal sealed class RawXbusCommand(byte[] xpressNetPacketIncludingXor) : Command
{
    private readonly byte[] _bytes = xpressNetPacketIncludingXor;
    internal override Frame ToFrame() => new(FrameHeader.Xbus, _bytes);
}

internal sealed class LoggingNotificationObserver(TestContext testContext)
    : IObserver<Tellurian.Trains.Communications.Interfaces.Notification>
{
    public List<Tellurian.Trains.Communications.Interfaces.Notification> Notifications { get; } = [];

    public void OnNext(Tellurian.Trains.Communications.Interfaces.Notification value)
    {
        Notifications.Add(value);
        testContext.WriteLine($"[notif] {value.GetType().Name}: {value}");
    }

    public void OnError(Exception error) => testContext.WriteLine($"[notif error] {error}");
    public void OnCompleted() => testContext.WriteLine("[notif completed]");
}

internal sealed class TestContextLogger<T>(TestContext testContext) : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        testContext.WriteLine($"[{logLevel.ToString()[0]}][{typeof(T).Name}] {formatter(state, exception)}");
        if (exception is not null) testContext.WriteLine($"  {exception}");
    }
}
