using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class LocoOperatedByAnotherDeviceNotificationTests
{
    [TestMethod]
    public void LocoOperatedByAnotherDevice_ParsesCorrectly()
    {
        var buffer = new byte[] { 0xE3, 0x40, 0xC4, 0xD2 };
        var notification = new LocoOperatedByAnotherDeviceNotification(buffer);

        Assert.AreEqual(0xE3, notification.Header);
        Assert.AreEqual(1234, notification.LocoAddress.Number);
    }

    [TestMethod]
    public void LocoOperatedByAnotherDevice_ParsesCorrectly_ShortAddress()
    {
        var buffer = new byte[] { 0xE3, 0x40, 0x00, 0x03 };
        var notification = new LocoOperatedByAnotherDeviceNotification(buffer);

        Assert.AreEqual(3, notification.LocoAddress.Number);
    }

    [TestMethod]
    public void LocoOperatedByAnotherDevice_CreatedByFactory()
    {
        var buffer = new byte[] { 0xE3, 0x40, 0x00, 0x03 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<LocoOperatedByAnotherDeviceNotification>(notification);
    }
}
