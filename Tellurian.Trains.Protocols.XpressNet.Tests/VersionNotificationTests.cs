using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class VersionNotificationTests
{
    [TestMethod]
    public void XpressNetVersion_ParsesPropertiesCorrectly()
    {
        var data = new byte[] { 0x63, 0x21, 0x30, 0x12, 0xFF };
        var actual = new VersionNotification(data);
        Assert.AreEqual(0x63, actual.Header);
        Assert.AreEqual(3, actual.Length);
        Assert.AreEqual("XpressNet", actual.BusName);
        Assert.AreEqual("3.0", actual.Version);
        Assert.AreEqual("Z21", actual.CommandStationName);
    }

    [TestMethod]
    public void XBusVersion_ParsesPropertiesCorrectly()
    {
        var data = new byte[] { 0x62, 0x21, 0x20, 0xFF };
        var actual = new VersionNotification(data);
        Assert.AreEqual(0x62, actual.Header);
        Assert.AreEqual(2, actual.Length);
        Assert.AreEqual("X-Bus", actual.BusName);
        Assert.AreEqual("2.0", actual.Version);
        Assert.AreEqual("Unknown", actual.CommandStationName);
    }
}
