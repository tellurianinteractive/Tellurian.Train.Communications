using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class CommandStationBusyNotificationTests
{
    [TestMethod]
    public void CommandStationBusyNotification_HasCorrectProperties()
    {
        var notification = new CommandStationBusyNotification();

        Assert.AreEqual(0x61, notification.Header);
        Assert.Contains("busy", CommandStationBusyNotification.Description);
    }

    [TestMethod]
    public void CommandStationBusyNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x61, 0x81 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<CommandStationBusyNotification>(notification);
    }
}
