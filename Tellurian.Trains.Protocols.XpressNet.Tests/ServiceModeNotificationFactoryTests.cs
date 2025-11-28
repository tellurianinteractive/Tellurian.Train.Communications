using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeNotificationFactoryTests
{
    [TestMethod]
    public void Factory_CreatesVersionNotification_ForHeader0x63Identification0x21()
    {
        var buffer = new byte[] { 0x63, 0x21, 0x30, 0x12, 0xFF };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(VersionNotification));
    }
}
