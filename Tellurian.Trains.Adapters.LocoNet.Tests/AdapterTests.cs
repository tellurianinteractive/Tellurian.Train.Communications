using Microsoft.Extensions.Logging.Abstractions;
using Tellurian.Trains.Communications.Channels;
using Tellurian.Trains.Communications.Interfaces.Extensions;
using Tellurian.Trains.Protocols.LocoNet;
using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Adapters.LocoNet.Tests;

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

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await adapter.SendAsync((Command)null!, TestContext.CancellationToken)
        );
    }

    [TestMethod]
    public async Task SendAsyncReturnsSuccessWhenChannelSucceeds()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.SendAsync(new PowerOnCommand(), TestContext.CancellationToken);

        Assert.IsTrue(result);
        Assert.HasCount(1, channel.SentData);
    }

    [TestMethod]
    public async Task SendAsyncReturnsFailureWhenChannelFails()
    {
        var channel = new MockChannel { ShouldFail = true };
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        var result = await adapter.SendAsync(new PowerOnCommand(), TestContext.CancellationToken);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ObserversReceiveNotificationsWhenSlotDataReceived()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // Simulate a slot notification (14 bytes)
        // OPC_SL_RD_DATA format: E7 0E <slot> <stat> <adr> <spd> <dirf> <trk> <ss2> <adr2> <snd> <id1> <id2> <chk>
        byte[] slotData = [0xE7, 0x0E, 0x01, 0x33, 0x05, 0x40, 0x20, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        slotData[13] = CalculateChecksum(slotData);
        channel.SimulateReceive(slotData);

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

        // Simulate a slot notification
        byte[] slotData = [0xE7, 0x0E, 0x01, 0x33, 0x0A, 0x40, 0x20, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        slotData[13] = CalculateChecksum(slotData);
        channel.SimulateReceive(slotData);

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

        byte[] slotData1 = [0xE7, 0x0E, 0x01, 0x33, 0x05, 0x40, 0x20, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        slotData1[13] = CalculateChecksum(slotData1);
        channel.SimulateReceive(slotData1);
        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);

        subscription.Dispose();

        byte[] slotData2 = [0xE7, 0x0E, 0x02, 0x33, 0x0A, 0x50, 0x20, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
        slotData2[13] = CalculateChecksum(slotData2);
        channel.SimulateReceive(slotData2);
        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);
    }

    [TestMethod]
    public void AdapterCanBeDisposed()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        adapter.Dispose();

        // Test passes if no exception is thrown
    }

    [TestMethod]
    public async Task AdapterCanBeDisposedAsync()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);

        await adapter.DisposeAsync();

        // Test passes if no exception is thrown
    }

    private static byte CalculateChecksum(byte[] data)
    {
        byte check = data[0];
        for (int i = 1; i < data.Length - 1; i++)
        {
            check ^= data[i];
        }
        return (byte)(~check);
    }
}
