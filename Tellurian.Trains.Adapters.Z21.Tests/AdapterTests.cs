using Microsoft.Extensions.Logging.Abstractions;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Interfaces.Decoder;
using Tellurian.Trains.Interfaces.Extensions;

namespace Tellurian.Trains.Adapters.Z21.Tests;

[TestClass]
public class AdapterTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task AdapterSubscribesToChannelOnStartReceive()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        Assert.AreEqual(0, channel.SubscriberCount);

        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        Assert.AreEqual(1, channel.SubscriberCount);
    }

    [TestMethod]
    public async Task SendAsyncWithNullCommandThrows()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await adapter.SendAsync((Command)null!, TestContext.CancellationToken)
        );
    }

    [TestMethod]
    public async Task SendAsyncReturnsSuccessWhenChannelSucceeds()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.SendAsync(new GetSerialNumberCommand(), TestContext.CancellationToken);

        Assert.IsTrue(result);
        Assert.HasCount(1, channel.SentData);
    }

    [TestMethod]
    public async Task SendAsyncReturnsFailureWhenChannelFails()
    {
        var channel = new MockChannel { ShouldFail = true };
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.SendAsync(new GetSerialNumberCommand(), TestContext.CancellationToken);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ObserversReceiveNotificationsWhenDataReceived()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // Simulate receiving a serial number notification
        // Frame format: [Length(2)] [Header(2)] [Data(4)]
        var serialNumber = 12345678;
        var frame = new Frame(FrameHeader.SerialNumber, BitConverter.GetBytes(serialNumber));
        var frameBytes = frame.GetBytes();

        channel.SimulateReceive(frameBytes);

        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);
    }

    [TestMethod]
    public async Task MultipleObserversAllReceiveNotifications()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer1 = new TestNotificationObserver();
        var observer2 = new TestNotificationObserver();
        var observer3 = new TestNotificationObserver();

        adapter.Subscribe(observer1);
        adapter.Subscribe(observer2);
        adapter.Subscribe(observer3);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        var serialNumber = 99999;
        var frame = new Frame(FrameHeader.SerialNumber, BitConverter.GetBytes(serialNumber));
        channel.SimulateReceive(frame.GetBytes());

        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer1.NotificationCount);
        Assert.AreEqual(1, observer2.NotificationCount);
        Assert.AreEqual(1, observer3.NotificationCount);
    }

    [TestMethod]
    public async Task UnsubscribeStopsReceivingNotifications()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        var subscription = adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // First notification
        var frame1 = new Frame(FrameHeader.SerialNumber, BitConverter.GetBytes(111));
        channel.SimulateReceive(frame1.GetBytes());
        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);

        // Unsubscribe
        subscription.Dispose();

        // Second notification
        var frame2 = new Frame(FrameHeader.SerialNumber, BitConverter.GetBytes(222));
        channel.SimulateReceive(frame2.GetBytes());
        await Task.Delay(50, TestContext.CancellationToken);

        // Should still be 1
        Assert.AreEqual(1, observer.NotificationCount);
    }
}

internal class MockChannel : ICommunicationsChannel
{
    private readonly Observers<CommunicationResult> _observers = new();
    public List<byte[]> SentData { get; } = [];
    public bool ShouldFail { get; set; }
    public int SubscriberCount => _observers.Count;

    public async Task<CommunicationResult> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);

        if (ShouldFail)
        {
            return CommunicationResult.Failure(new InvalidOperationException("Mock failure"));
        }

        SentData.Add(data);
        return CommunicationResult.Success(data, "Mock", "UDP");
    }

    public Task StartReceiveAsync(CancellationToken cancellationToken = default)
    {
        // Mock doesn't actually start receiving, tests call SimulateReceive manually
        return Task.CompletedTask;
    }

    public IDisposable Subscribe(IObserver<CommunicationResult> observer)
    {
        return _observers.Subscribe(observer);
    }

    public void SimulateReceive(byte[] data)
    {
        var result = CommunicationResult.Success(data, "Mock", "UDP");
        _observers.Notify(result);
    }
}

internal class TestNotificationObserver : IObserver<Interfaces.Notification>
{
    public int NotificationCount { get; private set; }
    public readonly List<Interfaces.Notification> Notifications = new List<Interfaces.Notification>();

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(Interfaces.Notification value)
    {
        NotificationCount++;
        Notifications.Add(value);
    }
}
