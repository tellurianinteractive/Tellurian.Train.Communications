using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class TurnoutInfoNotificationTests
{
    [TestMethod]
    public void CreatedByFactory_For0x43With4Bytes()
    {
        var buffer = new byte[] { 0x43, 0x00, 0x05, 0x02 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<TurnoutInfoNotification>(notification);
    }

    [TestMethod]
    public void FactoryStillReturnsFeedbackBroadcast_For0x43WithMoreBytes()
    {
        var buffer = new byte[] { 0x43, 0x05, 0x25, 0x06, 0x30, 0x07, 0x35 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<FeedbackBroadcast>(notification);
    }

    [TestMethod]
    public void ParsesWireAddressAndPosition()
    {
        // FAdr_MSB=0x00, FAdr_LSB=0x05 → wire address 5 (user address 6), position = Output2 (thrown)
        var buffer = new byte[] { 0x43, 0x00, 0x05, 0x02 };
        var notification = new TurnoutInfoNotification(buffer);

        Assert.AreEqual(5, notification.WireAddress);
        Assert.AreEqual(TurnoutPosition.Output2, notification.Position);
    }

    [TestMethod]
    public void MapsOutput1ToClosed()
    {
        // wire address 0 → user address 1, Output1 → ClosedOrGreen
        var notification = new TurnoutInfoNotification([0x43, 0x00, 0x00, 0x01]);

        var mapped = notification.Map();

        Assert.HasCount(1, mapped);
        var accessory = (AccessoryNotification)mapped[0];
        Assert.AreEqual((short)1, accessory.Address.Number);
        Assert.AreEqual(Position.ClosedOrGreen, accessory.Function);
    }

    [TestMethod]
    public void MapsOutput2ToThrown()
    {
        // wire address 41 → user address 42, Output2 → ThrownOrRed
        var notification = new TurnoutInfoNotification([0x43, 0x00, 0x29, 0x02]);

        var mapped = notification.Map();

        Assert.HasCount(1, mapped);
        var accessory = (AccessoryNotification)mapped[0];
        Assert.AreEqual((short)42, accessory.Address.Number);
        Assert.AreEqual(Position.ThrownOrRed, accessory.Function);
    }

    [TestMethod]
    public void MapsNotSwitchedToUnmapped()
    {
        var notification = new TurnoutInfoNotification([0x43, 0x00, 0x00, 0x00]);

        var mapped = notification.Map();

        Assert.HasCount(1, mapped);
        Assert.IsInstanceOfType<Communications.Interfaces.MessageNotification>(mapped[0]);
    }

    [TestMethod]
    public void MapsInvalidToUnmapped()
    {
        var notification = new TurnoutInfoNotification([0x43, 0x00, 0x00, 0x03]);

        var mapped = notification.Map();

        Assert.HasCount(1, mapped);
        Assert.IsInstanceOfType<Communications.Interfaces.MessageNotification>(mapped[0]);
    }

    [TestMethod]
    public void TwelveBitAddressParsedFromBothBytes()
    {
        // FAdr_MSB=0x07, FAdr_LSB=0xFF → wire 0x07FF = 2047 → user 2048 (max)
        var notification = new TurnoutInfoNotification([0x43, 0x07, 0xFF, 0x01]);

        Assert.AreEqual(0x07FF, notification.WireAddress);
        var mapped = notification.Map();
        var accessory = (AccessoryNotification)mapped[0];
        Assert.AreEqual((short)2048, accessory.Address.Number);
    }
}
