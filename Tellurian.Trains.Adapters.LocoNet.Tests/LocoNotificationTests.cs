using Microsoft.Extensions.Logging.Abstractions;
using Tellurian.Trains.Communications.Interfaces.Locos;

namespace Tellurian.Trains.Adapters.LocoNet.Tests;

[TestClass]
public class LocoNotificationTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task SlotNotification_MapsToLocoMovementNotification()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // OPC_SL_RD_DATA (0xE7) - Slot read data
        // Format: E7 0E <slot> <stat> <adr> <spd> <dirf> <trk> <ss2> <adr2> <snd> <id1> <id2> <chk>
        // For address 42, speed 64, forward direction
        byte[] slotData =
        [
            0xE7, 0x0E,  // Header and length
            0x01,        // Slot 1
            0x33,        // Status: IN_USE
            0x2A,        // ADR: Low address byte (42)
            0x41,        // SPD: Speed 65 (64 + 1 for protocol offset)
            0x20,        // DIRF: Forward direction (bit 5 set)
            0x07,        // TRK: Normal operation
            0x00,        // SS2
            0x00,        // ADR2: High address byte (0 for short address)
            0x00,        // SND
            0x00,        // ID1
            0x00,        // ID2
            0x00         // Checksum (to be calculated)
        ];
        slotData[13] = CalculateChecksum(slotData);
        channel.SimulateReceive(slotData);

        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);
        Assert.IsInstanceOfType<LocoMovementNotification>(observer.Notifications[0]);

        var locoNotification = (LocoMovementNotification)observer.Notifications[0];
        Assert.AreEqual(42, locoNotification.Address.Number);
        Assert.AreEqual(Direction.Forward, locoNotification.Direction);
    }

    [TestMethod]
    public async Task SlotNotification_WithReverseDirection_MapsCorrectly()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // For address 100, speed 32, reverse direction
        byte[] slotData =
        [
            0xE7, 0x0E,
            0x02,        // Slot 2
            0x33,        // IN_USE
            0x64,        // ADR: 100
            0x21,        // SPD: Speed 33
            0x00,        // DIRF: Reverse (bit 5 not set)
            0x07,
            0x00,
            0x00,        // Short address
            0x00,
            0x00,
            0x00,
            0x00
        ];
        slotData[13] = CalculateChecksum(slotData);
        channel.SimulateReceive(slotData);

        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);
        var locoNotification = (LocoMovementNotification)observer.Notifications[0];
        Assert.AreEqual(100, locoNotification.Address.Number);
        Assert.AreEqual(Direction.Backward, locoNotification.Direction);
    }

    [TestMethod]
    public async Task SlotNotification_WithLongAddress_MapsCorrectly()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // For long address 1234 (0x04D2)
        // In LocoNet: ADR = low 7 bits = 0x52, ADR2 = high 7 bits = 0x09
        byte[] slotData =
        [
            0xE7, 0x0E,
            0x03,        // Slot 3
            0x33,        // IN_USE
            0x52,        // ADR: Low 7 bits of 1234
            0x40,        // SPD: Speed 64
            0x20,        // DIRF: Forward
            0x07,
            0x00,
            0x09,        // ADR2: High 7 bits of 1234
            0x00,
            0x00,
            0x00,
            0x00
        ];
        slotData[13] = CalculateChecksum(slotData);
        channel.SimulateReceive(slotData);

        await Task.Delay(50, TestContext.CancellationToken);

        Assert.AreEqual(1, observer.NotificationCount);
        var locoNotification = (LocoMovementNotification)observer.Notifications[0];
        Assert.AreEqual(1234, locoNotification.Address.Number);
    }

    [TestMethod]
    public async Task ProgrammingSlotNotification_DoesNotMapToLocoNotification()
    {
        var channel = new MockChannel();
        var adapter = new Adapter(channel, NullLogger<Adapter>.Instance);
        var observer = new TestNotificationObserver();

        adapter.Subscribe(observer);
        await adapter.StartReceiveAsync(TestContext.CancellationToken);

        // Programming slot (124 = 0x7C)
        byte[] slotData =
        [
            0xE7, 0x0E,
            0x7C,        // Slot 124 (programming)
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00
        ];
        slotData[13] = CalculateChecksum(slotData);
        channel.SimulateReceive(slotData);

        await Task.Delay(50, TestContext.CancellationToken);

        // Programming slot should produce a different notification type (DecoderResponse)
        // or no notification if no pending programming request
        Assert.AreEqual(0, observer.Notifications.Count(n => n is LocoMovementNotification));
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
