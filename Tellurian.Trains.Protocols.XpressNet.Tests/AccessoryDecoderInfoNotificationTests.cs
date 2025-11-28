using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AccessoryDecoderInfoNotificationTests
{
    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesCorrectly()
    {
        var buffer = new byte[] { 0x42, 0x05, 0x25 };
        var notification = new AccessoryDecoderInfoNotification(buffer);

        Assert.AreEqual(0x42, notification.Header);
        Assert.AreEqual(0x05, notification.GroupAddress);
        Assert.AreEqual(0x25, notification.InfoByte);
        Assert.IsFalse(notification.IsIncomplete);
        Assert.AreEqual(AccessoryDecoderType.WithFeedback, notification.DecoderType);
        Assert.IsFalse(notification.IsUpperNibble);
        Assert.AreEqual(0x05, notification.StatusFlags);
        Assert.AreEqual(TurnoutStatus.Diverging, notification.FirstTurnoutStatus);
        Assert.AreEqual(TurnoutStatus.Diverging, notification.SecondTurnoutStatus);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesIncompleteFlag()
    {
        var buffer = new byte[] { 0x42, 0x00, 0x80 };
        var notification = new AccessoryDecoderInfoNotification(buffer);

        Assert.IsTrue(notification.IsIncomplete);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesDecoderTypes()
    {
        var buffer1 = new byte[] { 0x42, 0x00, 0x00 };
        Assert.AreEqual(AccessoryDecoderType.WithoutFeedback, new AccessoryDecoderInfoNotification(buffer1).DecoderType);

        var buffer2 = new byte[] { 0x42, 0x00, 0x20 };
        Assert.AreEqual(AccessoryDecoderType.WithFeedback, new AccessoryDecoderInfoNotification(buffer2).DecoderType);

        var buffer3 = new byte[] { 0x42, 0x00, 0x40 };
        Assert.AreEqual(AccessoryDecoderType.FeedbackModule, new AccessoryDecoderInfoNotification(buffer3).DecoderType);

        var buffer4 = new byte[] { 0x42, 0x00, 0x60 };
        Assert.AreEqual(AccessoryDecoderType.Reserved, new AccessoryDecoderInfoNotification(buffer4).DecoderType);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesNibble()
    {
        var buffer1 = new byte[] { 0x42, 0x00, 0x00 };
        Assert.IsFalse(new AccessoryDecoderInfoNotification(buffer1).IsUpperNibble);

        var buffer2 = new byte[] { 0x42, 0x00, 0x10 };
        Assert.IsTrue(new AccessoryDecoderInfoNotification(buffer2).IsUpperNibble);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesTurnoutStatus()
    {
        var buffer = new byte[] { 0x42, 0x00, 0x09 };
        var notification = new AccessoryDecoderInfoNotification(buffer);

        Assert.AreEqual(TurnoutStatus.Diverging, notification.FirstTurnoutStatus);
        Assert.AreEqual(TurnoutStatus.Straight, notification.SecondTurnoutStatus);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_CalculatesTurnoutAddresses()
    {
        var buffer1 = new byte[] { 0x42, 0x05, 0x00 };
        var notification1 = new AccessoryDecoderInfoNotification(buffer1);
        Assert.AreEqual(21, notification1.FirstTurnoutAddress);
        Assert.AreEqual(22, notification1.SecondTurnoutAddress);

        var buffer2 = new byte[] { 0x42, 0x05, 0x10 };
        var notification2 = new AccessoryDecoderInfoNotification(buffer2);
        Assert.AreEqual(23, notification2.FirstTurnoutAddress);
        Assert.AreEqual(24, notification2.SecondTurnoutAddress);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x42, 0x05, 0x25 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(AccessoryDecoderInfoNotification));
    }
}
