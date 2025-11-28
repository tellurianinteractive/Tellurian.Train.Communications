using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class TransferErrorNotificationTests
{
    [TestMethod]
    public void TransferErrorNotification_HasCorrectProperties()
    {
        var notification = new TransferErrorNotification();

        Assert.AreEqual(0x61, notification.Header);
        Assert.Contains("Transfer error", notification.Description);
    }

    [TestMethod]
    public void TransferErrorNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x61, 0x80 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<TransferErrorNotification>(notification);
    }
}
