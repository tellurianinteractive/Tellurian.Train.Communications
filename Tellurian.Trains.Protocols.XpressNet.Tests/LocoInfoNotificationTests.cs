using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class LocoInfoNotificationTests
{
    [TestMethod]
    public void LocoInfoNotification_ParsesPropertiesCorrectly()
    {
        var buffer = new byte[] { 0xEF, 0x00, 0x63, 0x0C, 0xFF, 0x13, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        var target = new LocoInfoNotification(buffer);
        Assert.AreEqual(99, target.Address.Number);
        Assert.IsTrue(target.IsControlledByOtherDevice);
    }
}
