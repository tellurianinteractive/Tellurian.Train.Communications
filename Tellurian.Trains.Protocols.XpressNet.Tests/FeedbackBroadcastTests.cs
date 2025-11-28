using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class FeedbackBroadcastTests
{
    [TestMethod]
    public void FeedbackBroadcast_CreatedByFactory_ForHeader0x41()
    {
        var buffer = new byte[] { 0x41, 0x05, 0x25 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(FeedbackBroadcast));
    }

    [TestMethod]
    public void FeedbackBroadcast_CreatedByFactory_ForHeader0x42With5Bytes()
    {
        var buffer = new byte[] { 0x42, 0x05, 0x25, 0x06, 0x30 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(FeedbackBroadcast));
    }

    [TestMethod]
    public void FeedbackBroadcast_CreatedByFactory_ForHeader0x43()
    {
        var buffer = new byte[] { 0x43, 0x05, 0x25, 0x06, 0x30, 0x07, 0x35 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(FeedbackBroadcast));
    }
}
