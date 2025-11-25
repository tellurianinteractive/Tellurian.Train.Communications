using System.Net;
using System.Net.Sockets;

namespace Tellurian.Communications.Channels.Tests;

[TestClass]
public class UdpChannelTests
{
    private UdpDataChannel? Target;
    private byte[]? ReceivedData;
    private IPEndPoint? Destination;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        Destination = new IPEndPoint(IPAddress.Loopback, 9901);
        Target = new UdpDataChannel(9902, Destination);
        ReceivedData = null;
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        if (Target is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            (Target as IDisposable)?.Dispose();
        }
    }

    [TestMethod]
    public async Task SendsPacket()
    {
        var listen = new IPEndPoint(IPAddress.Any, 9901);
        await (Target?.StartReceiveAsync(TestContext.CancellationToken) ?? Task.CompletedTask);
        using var receiver = new UdpClient(listen);
        var state = new UdpState { Client = receiver, Source = listen };
        receiver.BeginReceive(OnReceive, state);
        var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        await (Target?.SendAsync(data, TestContext.CancellationToken) ?? Task.FromResult(CommunicationResult.NoOperation()));
        await (Target?.SendAsync(data, TestContext.CancellationToken) ?? Task.FromResult(CommunicationResult.NoOperation()));
        await Task.Delay(100, TestContext.CancellationToken);
        Assert.IsNotNull(ReceivedData);
        Assert.AreEqual(data.Length, ReceivedData?.Length);
    }

    private void OnReceive(IAsyncResult result)
    {
        if (!(result.AsyncState is UdpState state)) return;
        var client = state.Client;
        var source = state.Source;
        ReceivedData = client is null ? Array.Empty<byte>() : client.EndReceive(result, ref source);
    }

    [TestMethod]
    public async Task ReceivesPacket()
    {
        var listener = new DataObserver();
        var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        using (var subscribtion = Target?.Subscribe(listener))
        {
            await (Target?.StartReceiveAsync(TestContext.CancellationToken) ?? Task.CompletedTask);
            using var sender = new UdpClient(9901);
            sender.Connect(IPAddress.Loopback, 9902);
            sender.Send(data, data.Length);
            sender.Send(data, data.Length);
            await Task.Delay(100, TestContext.CancellationToken);
        }
        Assert.IsNotNull(listener.ReceivedData);
        Assert.HasCount(2, listener.ReceivedData);
        Assert.AreEqual(data.Length, listener.ReceivedData[0].Length);
    }

    [TestMethod]
    public async Task SendAsyncWithNullDataReturnsNoOperation()
    {
        var result = await (Target?.SendAsync(null!, TestContext.CancellationToken) ?? Task.FromResult(CommunicationResult.NoOperation()));
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task SendAsyncWithEmptyDataReturnsNoOperation()
    {
        var result = await (Target?.SendAsync(Array.Empty<byte>(), TestContext.CancellationToken) ?? Task.FromResult(CommunicationResult.NoOperation()));
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task CancellationDuringReceiveStopsGracefully()
    {
        using var cts = new CancellationTokenSource();
        var listener = new DataObserver();

        using (var subscription = Target?.Subscribe(listener))
        {
            await (Target?.StartReceiveAsync(cts.Token) ?? Task.CompletedTask);
            await Task.Delay(50, TestContext.CancellationToken); // Let receive loop start

            // Cancel the operation
            cts.Cancel();

            // Give it time to process cancellation
            await Task.Delay(100, TestContext.CancellationToken);
        }

        // No assertion needed - test passes if no exception thrown
    }

    [TestMethod]
    public async Task DisposeAsyncWaitsForReceiveTaskCompletion()
    {
        using var cts = new CancellationTokenSource();
        var listener = new DataObserver();
        var channel = new UdpDataChannel(9903, new IPEndPoint(IPAddress.Loopback, 9904));

        using (var subscription = channel.Subscribe(listener))
        {
            await channel.StartReceiveAsync(cts.Token);
            await Task.Delay(50, TestContext.CancellationToken); // Let receive start

            cts.Cancel();
            await channel.DisposeAsync(); // Should wait for task to complete
        }

        // Test passes if DisposeAsync completes without hanging
    }

    [TestMethod]
    public async Task MultipleObserversAllReceiveNotifications()
    {
        var listener1 = new DataObserver();
        var listener2 = new DataObserver();
        var data = new byte[] { 1, 2, 3, 4 };

        using var sub1 = Target?.Subscribe(listener1);
        using var sub2 = Target?.Subscribe(listener2);

        await (Target?.StartReceiveAsync(TestContext.CancellationToken) ?? Task.CompletedTask);

        using var sender = new UdpClient(9901);
        sender.Connect(IPAddress.Loopback, 9902);
        sender.Send(data, data.Length);
        await Task.Delay(100, TestContext.CancellationToken);

        Assert.HasCount(1, listener1.ReceivedData);
        Assert.HasCount(1, listener2.ReceivedData);
    }

    [TestMethod]
    public async Task UnsubscribeStopsNotifications()
    {
        var listener = new DataObserver();
        var data = new byte[] { 1, 2, 3, 4 };

        var subscription = Target?.Subscribe(listener);
        await (Target?.StartReceiveAsync(TestContext.CancellationToken) ?? Task.CompletedTask);

        using var sender = new UdpClient(9901);
        sender.Connect(IPAddress.Loopback, 9902);
        sender.Send(data, data.Length);
        await Task.Delay(100, TestContext.CancellationToken);

        Assert.HasCount(1, listener.ReceivedData);

        // Unsubscribe
        subscription?.Dispose();

        // Send another packet
        sender.Send(data, data.Length);
        await Task.Delay(100, TestContext.CancellationToken);

        // Should still be 1 (no new notification received)
        Assert.HasCount(1, listener.ReceivedData);
    }

    [TestMethod]
    public async Task DisposeAsyncIsIdempotent()
    {
        var channel = new UdpDataChannel(9905, new IPEndPoint(IPAddress.Loopback, 9906));

        await channel.DisposeAsync();
        await channel.DisposeAsync(); // Second call should be no-op

        // Test passes if no exception thrown
    }

    [TestMethod]
    public void DisposeSyncIsIdempotent()
    {
        var channel = new UdpDataChannel(9907, new IPEndPoint(IPAddress.Loopback, 9908));

        channel.Dispose();
        channel.Dispose(); // Second call should be no-op

        // Test passes if no exception thrown
    }

    [TestMethod]
    public async Task ObserversReceiveCompletedOnDisposal()
    {
        var channel = new UdpDataChannel(9909, new IPEndPoint(IPAddress.Loopback, 9910));
        var observer = new CompletionTrackingObserver();

        channel.Subscribe(observer);
        Assert.IsFalse(observer.CompletedCalled);

        await channel.DisposeAsync();

        Assert.IsTrue(observer.CompletedCalled);
    }

    [TestMethod]
    public async Task SendAsyncAfterDisposeReturnsFailure()
    {
        var channel = new UdpDataChannel(9911, new IPEndPoint(IPAddress.Loopback, 9912));
        await channel.DisposeAsync();

        var data = new byte[] { 1, 2, 3 };
        var result = await channel.SendAsync(data, TestContext.CancellationToken);

        Assert.IsFalse(result.IsSuccess);
        Assert.IsInstanceOfType(result, typeof(FailureResult));
    }

    [TestMethod]
    public async Task ReceiveLoopStopsAfterSocketClose()
    {
        using var cts = new CancellationTokenSource();
        var channel = new UdpDataChannel(9913, new IPEndPoint(IPAddress.Loopback, 9914));
        var observer = new CompletionTrackingObserver();

        channel.Subscribe(observer);
        await channel.StartReceiveAsync(cts.Token);
        await Task.Delay(50, TestContext.CancellationToken);

        // Dispose will close the socket
        await channel.DisposeAsync();

        // Observer should have been notified of completion
        Assert.IsTrue(observer.CompletedCalled);
    }
}

internal class CompletionTrackingObserver : IObserver<CommunicationResult>
{
    public bool CompletedCalled { get; private set; }
    public bool ErrorCalled { get; private set; }
    public Exception? LastError { get; private set; }
    public int NotificationCount { get; private set; }

    public void OnCompleted()
    {
        CompletedCalled = true;
    }

    public void OnError(Exception error)
    {
        ErrorCalled = true;
        LastError = error;
    }

    public void OnNext(CommunicationResult value)
    {
        NotificationCount++;
    }
}

internal class DataObserver : IObserver<CommunicationResult>
{
    public readonly List<CommunicationResult> ReceivedData = new List<CommunicationResult>();

    void IObserver<CommunicationResult>.OnCompleted()
    {
    }

    void IObserver<CommunicationResult>.OnError(Exception error)
    {
        throw error;
    }

    void IObserver<CommunicationResult>.OnNext(CommunicationResult value)
    {
        ReceivedData.Add(value);
    }
}

