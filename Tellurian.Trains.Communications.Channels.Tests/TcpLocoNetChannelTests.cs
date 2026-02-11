using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tellurian.Trains.Communications.Channels.Tests;

[TestClass]
public class TcpLocoNetChannelTests
{
    private MockTcpStreamAdapter Stream = null!;
    private TcpLocoNetChannel Target = null!;
    private readonly ILogger<TcpLocoNetChannel> _logger = NullLogger<TcpLocoNetChannel>.Instance;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        Stream = new MockTcpStreamAdapter();
        Target = new TcpLocoNetChannel(Stream, _logger);
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await Target.DisposeAsync();
    }

    #region Hex encoding/decoding

    [TestMethod]
    public void FormatHexProducesCorrectUppercaseHex()
    {
        var result = TcpLocoNetChannel.FormatHex([0xA0, 0x2F, 0x00, 0x70]);
        Assert.AreEqual("A0 2F 00 70", result);
    }

    [TestMethod]
    public void FormatHexHandlesSingleByte()
    {
        var result = TcpLocoNetChannel.FormatHex([0xFF]);
        Assert.AreEqual("FF", result);
    }

    [TestMethod]
    public void ParseHexParsesValidHex()
    {
        var result = TcpLocoNetChannel.ParseHex("A0 2F 00 70");
        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(new byte[] { 0xA0, 0x2F, 0x00, 0x70 }, result);
    }

    [TestMethod]
    public void ParseHexReturnsNullForEmptyString()
    {
        Assert.IsNull(TcpLocoNetChannel.ParseHex(""));
    }

    [TestMethod]
    public void ParseHexReturnsNullForWhitespace()
    {
        Assert.IsNull(TcpLocoNetChannel.ParseHex("   "));
    }

    [TestMethod]
    public void ParseHexReturnsNullForInvalidHex()
    {
        Assert.IsNull(TcpLocoNetChannel.ParseHex("ZZ GG"));
    }

    [TestMethod]
    public void ParseHexHandlesExtraSpaces()
    {
        var result = TcpLocoNetChannel.ParseHex("A0  2F  00  70");
        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(new byte[] { 0xA0, 0x2F, 0x00, 0x70 }, result);
    }

    #endregion

    #region Receive

    [TestMethod]
    public async Task ReceiveLineNotifiesObserverWithCorrectBytes()
    {
        var observer = new DataObserver();
        using var subscription = Target.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Stream.EnqueueLine("RECEIVE A0 2F 00 70");
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.HasCount(1, observer.ReceivedData);
        var success = observer.ReceivedData[0] as SuccessResult;
        Assert.IsNotNull(success);
        CollectionAssert.AreEqual(new byte[] { 0xA0, 0x2F, 0x00, 0x70 }, success.Data());
    }

    [TestMethod]
    public async Task VersionLineStoresServerVersion()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Stream.EnqueueLine("VERSION LbServer 1.0");
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.AreEqual("LbServer 1.0", Target.ServerVersion);
    }

    [TestMethod]
    public async Task MultipleReceiveLinesAllNotifyObserver()
    {
        var observer = new DataObserver();
        using var subscription = Target.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Stream.EnqueueLine("RECEIVE A0 2F 00 70");
        Stream.EnqueueLine("RECEIVE B1 00 01 4F");
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.HasCount(2, observer.ReceivedData);
    }

    [TestMethod]
    public async Task InvalidHexInReceiveDoesNotNotifyObserver()
    {
        var observer = new DataObserver();
        using var subscription = Target.Subscribe(observer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await Target.StartReceiveAsync(cts.Token);

        Stream.EnqueueLine("RECEIVE ZZ GG");
        await Task.Delay(100, TestContext.CancellationToken);

        cts.Cancel();

        Assert.HasCount(0, observer.ReceivedData);
    }

    #endregion

    #region Send

    [TestMethod]
    public async Task SendAsyncFormatsCorrectSendLine()
    {
        var data = new byte[] { 0xA0, 0x2F, 0x00, 0x70 };
        var result = await Target.SendAsync(data, TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, Stream.WriteCount);
        Assert.AreEqual("SEND A0 2F 00 70", Stream.WriteHistory.First());
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

    #region Protocol tokens

    [TestMethod]
    public void SentOkHandledWithoutException()
    {
        Target.ProcessLine("SENT OK");
    }

    [TestMethod]
    public void SentErrorHandledWithoutException()
    {
        Target.ProcessLine("SENT ERROR timeout");
    }

    [TestMethod]
    public void TimestampHandledWithoutException()
    {
        Target.ProcessLine("TIMESTAMP 1234567890");
    }

    [TestMethod]
    public void BreakHandledWithoutException()
    {
        Target.ProcessLine("BREAK");
    }

    [TestMethod]
    public void ErrorHandledWithoutException()
    {
        Target.ProcessLine("ERROR CHECKSUM");
    }

    [TestMethod]
    public void ErrorLineHandledWithoutException()
    {
        Target.ProcessLine("ERROR LINE");
    }

    [TestMethod]
    public void ErrorMessageHandledWithoutException()
    {
        Target.ProcessLine("ERROR MESSAGE");
    }

    [TestMethod]
    public void UnknownTokenHandledWithoutException()
    {
        Target.ProcessLine("FOOBAR some data");
    }

    [TestMethod]
    public void EmptyLineHandledWithoutException()
    {
        Target.ProcessLine("");
    }

    #endregion

    #region Lifecycle

    [TestMethod]
    public async Task ServerDisconnectStopsReceiveLoop()
    {
        var observer = new CompletionTrackingObserver();
        using var subscription = Target.Subscribe(observer);

        await Target.StartReceiveAsync(TestContext.CancellationToken);

        Stream.SimulateDisconnect();
        await Task.Delay(100, TestContext.CancellationToken);

        // Dispose will complete the observer
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

    #endregion
}
