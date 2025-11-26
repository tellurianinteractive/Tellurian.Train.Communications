using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AccessoryInfoTests
{
    #region AccessoryInfoRequestCommand

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout1()
    {
        // Turnout 1 is in group 0, lower nibble
        var target = new AccessoryInfoRequestCommand(new AccessoryAddress(1));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);     // Header with length 2
        Assert.AreEqual(0x00, data[1]);     // Group address 0
        Assert.AreEqual(0x80, data[2]);     // Lower nibble (0x80 + 0)
    }

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout3()
    {
        // Turnout 3 is in group 0, upper nibble (subaddress 2)
        var target = new AccessoryInfoRequestCommand(new AccessoryAddress(3));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(0x00, data[1]);     // Group address 0
        Assert.AreEqual(0x81, data[2]);     // Upper nibble (0x80 + 1)
    }

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout5()
    {
        // Turnout 5 is in group 1, lower nibble
        var target = new AccessoryInfoRequestCommand(new AccessoryAddress(5));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(0x01, data[1]);     // Group address 1
        Assert.AreEqual(0x80, data[2]);     // Lower nibble
    }

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout100()
    {
        // Turnout 100: group = (100-1)/4 = 24, subaddress = 100 % 4 = 0 -> lower nibble
        var target = new AccessoryInfoRequestCommand(new AccessoryAddress(100));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(24, data[1]);       // Group address 24
        Assert.AreEqual(0x80, data[2]);     // Lower nibble
    }

    [TestMethod]
    public void AccessoryInfoRequest_ReturnsCorrectBytes_ForTurnout1024()
    {
        // Turnout 1024: group = (1024-1)/4 = 255, subaddress = 1024 % 4 = 0 -> lower nibble
        var target = new AccessoryInfoRequestCommand(new AccessoryAddress(1024));
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(255, data[1]);      // Group address 255
        Assert.AreEqual(0x80, data[2]);     // Lower nibble
    }

    [TestMethod]
    public void AccessoryInfoRequest_WithGroupAndNibble_ReturnsCorrectBytes()
    {
        var target = new AccessoryInfoRequestCommand(10, true);
        var data = target.GetData();

        Assert.AreEqual(0x42, data[0]);
        Assert.AreEqual(10, data[1]);       // Group address 10
        Assert.AreEqual(0x81, data[2]);     // Upper nibble
    }

    #endregion

    #region AccessoryDecoderInfoNotification

    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesCorrectly()
    {
        // Header=0x42, Address=5, Info=0x25 (no incomplete, without feedback, upper nibble, status 0101)
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
        // Info=0x80 (incomplete flag set)
        var buffer = new byte[] { 0x42, 0x00, 0x80 };
        var notification = new AccessoryDecoderInfoNotification(buffer);

        Assert.IsTrue(notification.IsIncomplete);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesDecoderTypes()
    {
        // Type 00 - without feedback
        var buffer1 = new byte[] { 0x42, 0x00, 0x00 };
        Assert.AreEqual(AccessoryDecoderType.WithoutFeedback, new AccessoryDecoderInfoNotification(buffer1).DecoderType);

        // Type 01 - with feedback
        var buffer2 = new byte[] { 0x42, 0x00, 0x20 };
        Assert.AreEqual(AccessoryDecoderType.WithFeedback, new AccessoryDecoderInfoNotification(buffer2).DecoderType);

        // Type 10 - feedback module
        var buffer3 = new byte[] { 0x42, 0x00, 0x40 };
        Assert.AreEqual(AccessoryDecoderType.FeedbackModule, new AccessoryDecoderInfoNotification(buffer3).DecoderType);

        // Type 11 - reserved
        var buffer4 = new byte[] { 0x42, 0x00, 0x60 };
        Assert.AreEqual(AccessoryDecoderType.Reserved, new AccessoryDecoderInfoNotification(buffer4).DecoderType);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesNibble()
    {
        // Lower nibble
        var buffer1 = new byte[] { 0x42, 0x00, 0x00 };
        Assert.IsFalse(new AccessoryDecoderInfoNotification(buffer1).IsUpperNibble);

        // Upper nibble
        var buffer2 = new byte[] { 0x42, 0x00, 0x10 };
        Assert.IsTrue(new AccessoryDecoderInfoNotification(buffer2).IsUpperNibble);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_ParsesTurnoutStatus()
    {
        // Status flags: Z3Z2=10, Z1Z0=01 -> second=straight, first=diverging
        var buffer = new byte[] { 0x42, 0x00, 0x09 };
        var notification = new AccessoryDecoderInfoNotification(buffer);

        Assert.AreEqual(TurnoutStatus.Diverging, notification.FirstTurnoutStatus);
        Assert.AreEqual(TurnoutStatus.Straight, notification.SecondTurnoutStatus);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_CalculatesTurnoutAddresses()
    {
        // Group 5, lower nibble -> turnouts 21, 22
        var buffer1 = new byte[] { 0x42, 0x05, 0x00 };
        var notification1 = new AccessoryDecoderInfoNotification(buffer1);
        Assert.AreEqual(21, notification1.FirstTurnoutAddress);
        Assert.AreEqual(22, notification1.SecondTurnoutAddress);

        // Group 5, upper nibble -> turnouts 23, 24
        var buffer2 = new byte[] { 0x42, 0x05, 0x10 };
        var notification2 = new AccessoryDecoderInfoNotification(buffer2);
        Assert.AreEqual(23, notification2.FirstTurnoutAddress);
        Assert.AreEqual(24, notification2.SecondTurnoutAddress);
    }

    [TestMethod]
    public void AccessoryDecoderInfoNotification_CreatedByFactory()
    {
        // Buffer with 3 bytes -> AccessoryDecoderInfoNotification
        var buffer = new byte[] { 0x42, 0x05, 0x25 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(AccessoryDecoderInfoNotification));
    }

    #endregion

    #region FeedbackBroadcast via Factory

    [TestMethod]
    public void FeedbackBroadcast_CreatedByFactory_ForHeader0x41()
    {
        // Header 0x41 = 1 pair (3 bytes: header + 2 data bytes)
        var buffer = new byte[] { 0x41, 0x05, 0x25 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(FeedbackBroadcast));
    }

    [TestMethod]
    public void FeedbackBroadcast_CreatedByFactory_ForHeader0x42With5Bytes()
    {
        // Header 0x42 with 5 bytes = FeedbackBroadcast with 2 pairs
        var buffer = new byte[] { 0x42, 0x05, 0x25, 0x06, 0x30 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(FeedbackBroadcast));
    }

    [TestMethod]
    public void FeedbackBroadcast_CreatedByFactory_ForHeader0x43()
    {
        // Header 0x43 = 3 pairs (7 bytes)
        var buffer = new byte[] { 0x43, 0x05, 0x25, 0x06, 0x30, 0x07, 0x35 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(FeedbackBroadcast));
    }

    #endregion
}
