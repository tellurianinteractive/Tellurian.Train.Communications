using Microsoft.Extensions.Logging.Abstractions;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Communications.Interfaces.Decoder;
using Tellurian.Trains.Communications.Interfaces.Extensions;

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
    public async Task XpressNetAccessorySendPreDeactivatesOppositeThenActivatesDesired()
    {
        // The adapter pre-deactivates the opposite output to clear Z21's in-flight tracking,
        // then activates the desired output. No background deactivate needed — self-deactivating
        // decoders (e.g. Möllehem stall-motor) handle motor timing internally.
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance, BroadcastSubjects.None, useLocoNetForAccessories: false);

        var sent = await adapter.SetAccessoryAsync(Address.From(802), AccessoryCommand.Throw(), TestContext.CancellationToken);

        Assert.IsTrue(sent);
        Assert.HasCount(2, channel.SentData);
        // First: pre-deactivate opposite output (Port2/P=1, A=0) → DB2 = 0x81
        Assert.AreEqual(0x81, channel.SentData[0][7], "pre-deactivate opposite output P=1 A=0");
        // Second: activate desired output (Port1/P=0, A=1) → DB2 = 0x88
        Assert.AreEqual(0x88, channel.SentData[1][7], "activate desired output P=0 A=1");
    }

    [TestMethod]
    public async Task XpressNetAccessorySendOnlySendsDeactivateWhenExplicitOff()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance, BroadcastSubjects.None, useLocoNetForAccessories: false);

        var sent = await adapter.SetAccessoryAsync(Address.From(802), AccessoryCommand.Throw(activate: false), TestContext.CancellationToken);

        Assert.IsTrue(sent);
        Assert.HasCount(1, channel.SentData);
        // Throw = ThrownOrRed → Port1 (P=0), deactivate A=0 → DB2 = 0x80
        Assert.AreEqual(0x80, channel.SentData[0][7], "only deactivate should be sent (DB2 bit 3 clear)");
    }

    [TestMethod]
    public async Task UnsubscribeStopsReceivingNotifications()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        var subscription = adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        var frame1 = new Frame(FrameHeader.SerialNumber, BitConverter.GetBytes(111));
        channel.SimulateReceive(frame1.GetBytes());
        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);

        subscription.Dispose();

        var frame2 = new Frame(FrameHeader.SerialNumber, BitConverter.GetBytes(222));
        channel.SimulateReceive(frame2.GetBytes());
        await Task.Delay(50, TestContext.CancellationToken);

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

internal class TestNotificationObserver : IObserver<Tellurian.Trains.Communications.Interfaces.Notification>
{
    public int NotificationCount { get; private set; }
    public readonly List<Tellurian.Trains.Communications.Interfaces.Notification> Notifications = new List<Tellurian.Trains.Communications.Interfaces.Notification>();

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(Tellurian.Trains.Communications.Interfaces.Notification value)
    {
        NotificationCount++;
        Notifications.Add(value);
    }
}
