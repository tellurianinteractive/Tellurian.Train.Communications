using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tellurian.Trains.Communications.Channels.Tests;

[TestClass]
public class UdpLocoNetChannelTests
{
    private MockUdpLocoNetAdapter Adapter = null!;
    private UdpLocoNetChannel Target = null!;
    private readonly ILogger<UdpLocoNetChannel> _logger = NullLogger<UdpLocoNetChannel>.Instance;

    // Valid LocoNet message: opcode 0xB2, data 0x1F 0x00, checksum 0x52
    // XOR: 0xB2 ^ 0x1F ^ 0x00 ^ 0x52 = 0xFF
    private static readonly byte[] ValidMessage = [0xB2, 0x1F, 0x00, 0x52];

    // Another valid LocoNet message: opcode 0xA0, data 0x03, checksum 0x5C
    // XOR: 0xA0 ^ 0x03 ^ 0x5C = 0xFF
    private static readonly byte[] ValidMessage2 = [0xA0, 0x03, 0x5C];

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        Adapter = new MockUdpLocoNetAdapter();
        Target = new UdpLocoNetChannel(Adapter, _logger);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await Target.DisposeAsync();
    }

    #region Validation

    [TestMethod]
    public void ValidLocoNetMessagePassesValidation()
    {
        Assert.IsTrue(UdpLocoNetChannel.IsValidLocoNetMessage(ValidMessage));
    }

    [TestMethod]
    public void EmptyArrayFailsValidation()
    {
        Assert.IsFalse(UdpLocoNetChannel.IsValidLocoNetMessage([]));
    }

    [TestMethod]
    public void SingleByteFailsValidation()
    {
        Assert.IsFalse(UdpLocoNetChannel.IsValidLocoNetMessage([0xB2]));
    }

    [TestMethod]
    public void BadChecksumFailsValidation()
    {
        // Change last byte so XOR != 0xFF
        Assert.IsFalse(UdpLocoNetChannel.IsValidLocoNetMessage([0xB2, 0x1F, 0x00, 0x00]));
    }

    [TestMethod]
    public void FirstByteWithoutMsbFailsValidation()
    {
        // First byte 0x32 has MSB=0, not a valid opcode
        Assert.IsFalse(UdpLocoNetChannel.IsValidLocoNetMessage([0x32, 0x1F, 0x00, 0x52]));
    }

    [TestMethod]
    public void NullFailsValidation()
    {
        Assert.IsFalse(UdpLocoNetChannel.IsValidLocoNetMessage(null!));
    }

    #endregion

    #region Receive

    [TestMethod]
    public async Task ValidDatagramNotifiesObserverWithCorrectBytes()
    {
        var observer = new DataObserver();
        using var subscription = Target.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Adapter.EnqueueDatagram(ValidMessage);
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.HasCount(1, observer.ReceivedData);
        var success = observer.ReceivedData[0] as SuccessResult;
        Assert.IsNotNull(success);
        CollectionAssert.AreEqual(ValidMessage, success.Data());
        Assert.AreEqual("LocoNetUDP", success.ProtocolName);
    }

    [TestMethod]
    public async Task MultipleDatagramsAllNotifyObserver()
    {
        var observer = new DataObserver();
        using var subscription = Target.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Adapter.EnqueueDatagram(ValidMessage);
        Adapter.EnqueueDatagram(ValidMessage2);
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.HasCount(2, observer.ReceivedData);
    }

    [TestMethod]
    public async Task InvalidChecksumDatagramDoesNotNotifyObserver()
    {
        var observer = new DataObserver();
        using var subscription = Target.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Adapter.EnqueueDatagram([0xB2, 0x1F, 0x00, 0x00]); // Bad checksum
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.HasCount(0, observer.ReceivedData);
    }

    [TestMethod]
    public async Task EmptyDatagramDoesNotNotifyObserver()
    {
        var observer = new DataObserver();
        using var subscription = Target.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Adapter.EnqueueDatagram([]);
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.HasCount(0, observer.ReceivedData);
    }

    [TestMethod]
    public async Task DatagramWithBadOpcodeDoesNotNotifyObserver()
    {
        var observer = new DataObserver();
        using var subscription = Target.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        // First byte 0x32 has MSB=0
        Adapter.EnqueueDatagram([0x32, 0x1F, 0x00, 0x52]);
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.HasCount(0, observer.ReceivedData);
    }

    #endregion

    #region Send

    [TestMethod]
    public async Task SendAsyncPassesRawBytesToAdapter()
    {
        var result = await Target.SendAsync(ValidMessage, TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, Adapter.SendCount);
        CollectionAssert.AreEqual(ValidMessage, Adapter.SentData.First());
    }

    [TestMethod]
    public async Task SendAsyncWithNullDataReturnsNoOperation()
    {
        var result = await Target.SendAsync(null!, TestContext.CancellationToken);
        Assert.IsInstanceOfType<NoOperationResult>(result);
    }

    [TestMethod]
    public async Task SendAsyncWithEmptyDataReturnsNoOperation()
    {
        var result = await Target.SendAsync([], TestContext.CancellationToken);
        Assert.IsInstanceOfType<NoOperationResult>(result);
    }

    #endregion

    #region Lifecycle

    [TestMethod]
    public async Task AdapterCloseStopsReceiveLoop()
    {
        var observer = new CompletionTrackingObserver();
        using var subscription = Target.Subscribe(observer);

        await Target.StartReceiveAsync(TestContext.CancellationToken);

        Adapter.SimulateClose();
        await Task.Delay(100, TestContext.CancellationToken);

        await Target.DisposeAsync();
        Assert.IsTrue(observer.CompletedCalled);
    }

    [TestMethod]
    public async Task CancellationStopsReceiveGracefully()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);
        await Task.Delay(50, TestContext.CancellationToken);

        cts.Cancel();
        await Task.Delay(100, TestContext.CancellationToken);

        // Test passes if no exception thrown
    }

    [TestMethod]
    public async Task DisposeAsyncIsIdempotent()
    {
        await Target.DisposeAsync();
        await Target.DisposeAsync(); // Second call should be no-op
    }

    [TestMethod]
    public void DisposeSyncIsIdempotent()
    {
        Target.Dispose();
        Target.Dispose(); // Second call should be no-op
    }

    [TestMethod]
    public async Task ObserverReceivesCompletedOnDisposal()
    {
        var observer = new CompletionTrackingObserver();
        Target.Subscribe(observer);
        Assert.IsFalse(observer.CompletedCalled);

        await Target.DisposeAsync();

        Assert.IsTrue(observer.CompletedCalled);
    }

    [TestMethod]
    public async Task StartReceiveAutoJoinsAdapter()
    {
        Assert.IsFalse(Adapter.IsJoined);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Assert.IsTrue(Adapter.IsJoined);

        cts.Cancel();
    }

    [TestMethod]
    public async Task SendAsyncAutoJoinsAdapter()
    {
        Assert.IsFalse(Adapter.IsJoined);

        await Target.SendAsync(ValidMessage, TestContext.CancellationToken);

        Assert.IsTrue(Adapter.IsJoined);
    }

    #endregion

    #region Checksum validation disabled

    [TestMethod]
    public async Task WithValidationDisabledBadChecksumIsAccepted()
    {
        await Target.DisposeAsync();
        var channel = new UdpLocoNetChannel(Adapter = new MockUdpLocoNetAdapter(), _logger, validateChecksum: false);

        var observer = new DataObserver();
        using var subscription = channel.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await channel.StartReceiveAsync(cts.Token);

        // Bad checksum but MSB=1, length >= 2
        Adapter.EnqueueDatagram([0xB2, 0x1F, 0x00, 0x00]);
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();
        await channel.DisposeAsync();

        Assert.HasCount(1, observer.ReceivedData);
    }

    #endregion
}
