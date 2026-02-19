using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class MultiSenseNotificationTests
{
    private static byte[] CreateMessage(byte b1, byte b2, byte b3, byte b4)
    {
        byte[] data = [MultiSenseNotification.OperationCode, b1, b2, b3, b4, 0x00];
        data[5] = Message.Checksum(data);
        return data;
    }

    [TestMethod]
    public void ParsesTransponderPresent_WithShortAddress()
    {
        // Type = 0x20 (present), section high=0, section low=5, short address marker 0x7D, loco=3
        var data = CreateMessage(0x20, 0x05, 0x7D, 0x03);
        var notification = new MultiSenseNotification(data);

        Assert.IsTrue(notification.IsTransponding);
        Assert.IsTrue(notification.IsPresent);
        Assert.AreEqual((ushort)5, notification.Section);
        Assert.AreEqual((ushort)3, notification.LocoAddress);
    }

    [TestMethod]
    public void ParsesTransponderAbsent_WithLongAddress()
    {
        // Type = 0x00 (absent), address bits, long address = (0x01 << 7) | 0x03 = 131
        var data = CreateMessage(0x00, 0x0A, 0x01, 0x03);
        var notification = new MultiSenseNotification(data);

        Assert.IsTrue(notification.IsTransponding);
        Assert.IsFalse(notification.IsPresent);
        Assert.AreEqual((ushort)131, notification.LocoAddress);
    }

    [TestMethod]
    public void ParsesPowerManagementMessage()
    {
        var data = CreateMessage(0x60, 0x05, 0x00, 0x00);
        var notification = new MultiSenseNotification(data);

        Assert.IsFalse(notification.IsTransponding);
        Assert.IsTrue(notification.IsPowerMessage);
    }

    [TestMethod]
    public void ParsesSectionAddress()
    {
        // Section = (0x02 & 0x1F) << 7 | (0x10 & 0x7F) = 0x100 | 0x10 = 272
        var data = CreateMessage(0x22, 0x10, 0x7D, 0x01);
        var notification = new MultiSenseNotification(data);

        Assert.AreEqual((ushort)272, notification.Section);
    }

    [TestMethod]
    public void ParsesZone()
    {
        // Zone from lower nibble of data[2]: 3 => 'D'
        var data = CreateMessage(0x20, 0x03, 0x7D, 0x01);
        var notification = new MultiSenseNotification(data);

        Assert.AreEqual('D', notification.Zone);
    }

    [TestMethod]
    public void Factory_Creates_MultiSenseNotification()
    {
        var data = CreateMessage(0x20, 0x05, 0x7D, 0x03);
        var message = LocoNetMessageFactory.Create(data);

        Assert.IsInstanceOfType<MultiSenseNotification>(message);
    }
}
