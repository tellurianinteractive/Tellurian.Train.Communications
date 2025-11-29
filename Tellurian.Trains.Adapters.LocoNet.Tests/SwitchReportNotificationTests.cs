using Microsoft.Extensions.Logging.Abstractions;
using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Adapters.LocoNet.Tests;

[TestClass]
public class SwitchReportNotificationTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task SwitchReportNotification_MapsToAccessoryNotification()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // OPC_SW_REP (0xB1) - Switch report with closed position
        // Format: B1 <sn1> <sn2> <chk>
        // sn1 = address bits 0-6
        // sn2 = address bits 7-10 + flags
        // For address 10, closed output on: sn1=0x0A, sn2=0x20 (C=1, T=0)
        byte[] switchReport = [0xB1, 0x0A, 0x20, 0x00];
        switchReport[3] = CalculateChecksum(switchReport);
        channel.SimulateReceive(switchReport);

        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);
        Assert.IsInstanceOfType<AccessoryNotification>(observer.Notifications[0]);

        var accessoryNotification = (AccessoryNotification)observer.Notifications[0];
        Assert.AreEqual(10, accessoryNotification.Address.Number);
        Assert.AreEqual(Position.ClosedOrGreen, accessoryNotification.Function);
    }

    [TestMethod]
    public async Task SwitchReportNotification_WithThrownPosition_MapsCorrectly()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // Switch report with thrown position
        // For address 5, thrown output on: sn1=0x05, sn2=0x10 (C=0, T=1)
        byte[] switchReport = [0xB1, 0x05, 0x10, 0x00];
        switchReport[3] = CalculateChecksum(switchReport);
        channel.SimulateReceive(switchReport);

        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);
        var accessoryNotification = (AccessoryNotification)observer.Notifications[0];
        Assert.AreEqual(5, accessoryNotification.Address.Number);
        Assert.AreEqual(Position.ThrownOrRed, accessoryNotification.Function);
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
