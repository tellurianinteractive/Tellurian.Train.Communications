using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet.Tests;

[TestClass]
public class LissyNotificationTests
{
    private static byte[] CreateLissyMessage(byte sectionAddress, byte addrHigh, byte addrLow, byte directionAndCategory)
    {
        // OPC=0xE4, subtype=0x08, section, addrH, addrL, dirCat, 0x00 (checksum placeholder)
        byte[] data = [LissyNotification.OperationCode, 0x08, sectionAddress, addrHigh, addrLow, directionAndCategory, 0x00];
        data[6] = Message.Checksum(data);
        return data;
    }

    [TestMethod]
    public void ParsesLocoAddress()
    {
        // Address = (0x01 << 7) | 0x03 = 131
        var data = CreateLissyMessage(0x05, 0x01, 0x03, 0x20);
        var notification = new LissyNotification(data);

        Assert.AreEqual((ushort)131, notification.LocoAddress);
        Assert.IsTrue(notification.IsValid);
    }

    [TestMethod]
    public void ParsesSectionAddress()
    {
        var data = CreateLissyMessage(0x0A, 0x00, 0x03, 0x00);
        var notification = new LissyNotification(data);

        Assert.AreEqual((byte)0x0A, notification.SectionAddress);
    }

    [TestMethod]
    public void ParsesForwardDirection()
    {
        var data = CreateLissyMessage(0x05, 0x00, 0x03, 0x20); // Bit 5 set = forward
        var notification = new LissyNotification(data);

        Assert.IsTrue(notification.IsForward);
    }

    [TestMethod]
    public void ParsesReverseDirection()
    {
        var data = CreateLissyMessage(0x05, 0x00, 0x03, 0x00); // Bit 5 clear = reverse
        var notification = new LissyNotification(data);

        Assert.IsFalse(notification.IsForward);
    }

    [TestMethod]
    public void ParsesCategory()
    {
        var data = CreateLissyMessage(0x05, 0x00, 0x03, 0x05); // Category = 5
        var notification = new LissyNotification(data);

        Assert.AreEqual((byte)5, notification.Category);
    }

    [TestMethod]
    public void IsLissyMessage_ReturnsTrueForLissySubType()
    {
        byte[] data = [0xE4, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00];
        Assert.IsTrue(LissyNotification.IsLissyMessage(data));
    }

    [TestMethod]
    public void IsLissyMessage_ReturnsFalseForOtherSubType()
    {
        byte[] data = [0xE4, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00];
        Assert.IsFalse(LissyNotification.IsLissyMessage(data));
    }

    [TestMethod]
    public void InvalidLocoAddress_IsNotValid()
    {
        var data = CreateLissyMessage(0x05, 0x00, 0x00, 0x00); // Address = 0
        var notification = new LissyNotification(data);

        Assert.IsFalse(notification.IsValid);
    }

    [TestMethod]
    public void Factory_Creates_LissyNotification()
    {
        var data = CreateLissyMessage(0x05, 0x00, 0x03, 0x20);
        var message = LocoNetMessageFactory.Create(data);

        Assert.IsInstanceOfType<LissyNotification>(message);
    }
}
