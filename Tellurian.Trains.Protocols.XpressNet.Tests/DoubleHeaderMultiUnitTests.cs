using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class DoubleHeaderMultiUnitTests
{
    #region EstablishDoubleHeaderCommand

    [TestMethod]
    public void EstablishDoubleHeader_ReturnsCorrectBytes_ForShortAddresses()
    {
        var target = new EstablishDoubleHeaderCommand(new LocoAddress(3), new LocoAddress(5));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(0x00, data[4]);
        Assert.AreEqual(0x05, data[5]);
    }

    [TestMethod]
    public void EstablishDoubleHeader_ReturnsCorrectBytes_ForLongAddresses()
    {
        var target = new EstablishDoubleHeaderCommand(new LocoAddress(1234), new LocoAddress(5678));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        Assert.AreEqual(0xD6, data[4]);
        Assert.AreEqual(0x2E, data[5]);
    }

    #endregion

    #region DissolveDoubleHeaderCommand

    [TestMethod]
    public void DissolveDoubleHeader_ReturnsCorrectBytes_ForShortAddress()
    {
        var target = new DissolveDoubleHeaderCommand(new LocoAddress(3));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(0x00, data[4]);
        Assert.AreEqual(0x00, data[5]);
    }

    [TestMethod]
    public void DissolveDoubleHeader_ReturnsCorrectBytes_ForLongAddress()
    {
        var target = new DissolveDoubleHeaderCommand(new LocoAddress(1234));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        Assert.AreEqual(0x00, data[4]);
        Assert.AreEqual(0x00, data[5]);
    }

    #endregion

    #region AddLocoToMultiUnitCommand

    [TestMethod]
    public void AddLocoToMultiUnit_ReturnsCorrectBytes_SameDirection()
    {
        var target = new AddLocoToMultiUnitCommand(new LocoAddress(3), 10, reversed: false);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x40, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(10, data[4]);
    }

    [TestMethod]
    public void AddLocoToMultiUnit_ReturnsCorrectBytes_ReversedDirection()
    {
        var target = new AddLocoToMultiUnitCommand(new LocoAddress(3), 10, reversed: true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x41, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(10, data[4]);
    }

    [TestMethod]
    public void AddLocoToMultiUnit_ReturnsCorrectBytes_ForLongAddress()
    {
        var target = new AddLocoToMultiUnitCommand(new LocoAddress(1234), 99);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x40, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        Assert.AreEqual(99, data[4]);
    }

    [TestMethod]
    public void AddLocoToMultiUnit_Throws_WhenMTRAddressIsZero()
    {
        try
        {
            _ = new AddLocoToMultiUnitCommand(new LocoAddress(3), 0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void AddLocoToMultiUnit_Throws_WhenMTRAddressTooHigh()
    {
        try
        {
            _ = new AddLocoToMultiUnitCommand(new LocoAddress(3), 100);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region RemoveLocoFromMultiUnitCommand

    [TestMethod]
    public void RemoveLocoFromMultiUnit_ReturnsCorrectBytes()
    {
        var target = new RemoveLocoFromMultiUnitCommand(new LocoAddress(3), 10);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x42, data[1]);
        Assert.AreEqual(0x00, data[2]);
        Assert.AreEqual(0x03, data[3]);
        Assert.AreEqual(10, data[4]);
    }

    [TestMethod]
    public void RemoveLocoFromMultiUnit_ReturnsCorrectBytes_ForLongAddress()
    {
        var target = new RemoveLocoFromMultiUnitCommand(new LocoAddress(1234), 50);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x42, data[1]);
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        Assert.AreEqual(50, data[4]);
    }

    [TestMethod]
    public void RemoveLocoFromMultiUnit_Throws_WhenMTRAddressIsZero()
    {
        try
        {
            _ = new RemoveLocoFromMultiUnitCommand(new LocoAddress(3), 0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region MUDHErrorNotification

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_NotOperatedByDevice()
    {
        var buffer = new byte[] { 0xE1, 0x81 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(0xE1, notification.Header);
        Assert.AreEqual(0x81, notification.IdentificationByte);
        Assert.AreEqual(MUDHErrorCode.NotOperatedByDevice, notification.ErrorCode);
        Assert.Contains("not been operated", notification.ErrorMessage);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_OperatedByAnotherDevice()
    {
        var buffer = new byte[] { 0xE1, 0x82 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.OperatedByAnotherDevice, notification.ErrorCode);
        Assert.Contains("another XpressNet device", notification.ErrorMessage);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_AlreadyInConsist()
    {
        var buffer = new byte[] { 0xE1, 0x83 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.AlreadyInConsist, notification.ErrorCode);
        Assert.Contains("already in another", notification.ErrorMessage);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_SpeedNotZero()
    {
        var buffer = new byte[] { 0xE1, 0x84 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.SpeedNotZero, notification.ErrorCode);
        Assert.Contains("speed", notification.ErrorMessage);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_NotInMultiUnit()
    {
        var buffer = new byte[] { 0xE1, 0x85 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.NotInMultiUnit, notification.ErrorCode);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_NotMultiUnitBaseAddress()
    {
        var buffer = new byte[] { 0xE1, 0x86 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.NotMultiUnitBaseAddress, notification.ErrorCode);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_CannotDelete()
    {
        var buffer = new byte[] { 0xE1, 0x87 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.CannotDelete, notification.ErrorCode);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_StackFull()
    {
        var buffer = new byte[] { 0xE1, 0x88 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.StackFull, notification.ErrorCode);
        Assert.Contains("stack is full", notification.ErrorMessage);
    }

    [TestMethod]
    public void MUDHErrorNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0xE1, 0x81 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<MultiUnitAndDoubleHeaderErrorNotification>(notification);
    }

    #endregion
}
