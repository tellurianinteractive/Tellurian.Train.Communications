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

        Assert.AreEqual(0xE5, data[0]);     // Header with length 5
        Assert.AreEqual(0x43, data[1]);     // Identification
        Assert.AreEqual(0x00, data[2]);     // Address 1 High
        Assert.AreEqual(0x03, data[3]);     // Address 1 Low
        Assert.AreEqual(0x00, data[4]);     // Address 2 High
        Assert.AreEqual(0x05, data[5]);     // Address 2 Low
    }

    [TestMethod]
    public void EstablishDoubleHeader_ReturnsCorrectBytes_ForLongAddresses()
    {
        var target = new EstablishDoubleHeaderCommand(new LocoAddress(1234), new LocoAddress(5678));
        var data = target.GetData();

        Assert.AreEqual(0xE5, data[0]);
        Assert.AreEqual(0x43, data[1]);
        // Address 1234 = 0x04D2, with long address flag 0xC0 -> 0xC4, 0xD2
        Assert.AreEqual(0xC4, data[2]);
        Assert.AreEqual(0xD2, data[3]);
        // Address 5678 = 0x162E, with long address flag -> 0xD6, 0x2E
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

        Assert.AreEqual(0xE5, data[0]);     // Header with length 5
        Assert.AreEqual(0x43, data[1]);     // Identification
        Assert.AreEqual(0x00, data[2]);     // Address High
        Assert.AreEqual(0x03, data[3]);     // Address Low
        Assert.AreEqual(0x00, data[4]);     // Second address = 0 (dissolve marker)
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

        Assert.AreEqual(0xE4, data[0]);     // Header with length 4
        Assert.AreEqual(0x40, data[1]);     // Identification (R=0)
        Assert.AreEqual(0x00, data[2]);     // Loco Address High
        Assert.AreEqual(0x03, data[3]);     // Loco Address Low
        Assert.AreEqual(10, data[4]);       // MTR address
    }

    [TestMethod]
    public void AddLocoToMultiUnit_ReturnsCorrectBytes_ReversedDirection()
    {
        var target = new AddLocoToMultiUnitCommand(new LocoAddress(3), 10, reversed: true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x41, data[1]);     // Identification (R=1)
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

        Assert.AreEqual(0xE4, data[0]);     // Header with length 4
        Assert.AreEqual(0x42, data[1]);     // Identification for remove
        Assert.AreEqual(0x00, data[2]);     // Loco Address High
        Assert.AreEqual(0x03, data[3]);     // Loco Address Low
        Assert.AreEqual(10, data[4]);       // MTR address
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
        var notification = new MUDHErrorNotification(buffer);

        Assert.AreEqual(0xE1, notification.Header);
        Assert.AreEqual(0x81, notification.IdentificationByte);
        Assert.AreEqual(MUDHErrorCode.NotOperatedByDevice, notification.ErrorCode);
        Assert.IsTrue(notification.ErrorMessage.Contains("not been operated"));
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_OperatedByAnotherDevice()
    {
        var buffer = new byte[] { 0xE1, 0x82 };
        var notification = new MUDHErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.OperatedByAnotherDevice, notification.ErrorCode);
        Assert.IsTrue(notification.ErrorMessage.Contains("another XpressNet device"));
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_AlreadyInConsist()
    {
        var buffer = new byte[] { 0xE1, 0x83 };
        var notification = new MUDHErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.AlreadyInConsist, notification.ErrorCode);
        Assert.IsTrue(notification.ErrorMessage.Contains("already in another"));
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_SpeedNotZero()
    {
        var buffer = new byte[] { 0xE1, 0x84 };
        var notification = new MUDHErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.SpeedNotZero, notification.ErrorCode);
        Assert.IsTrue(notification.ErrorMessage.Contains("speed"));
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_NotInMultiUnit()
    {
        var buffer = new byte[] { 0xE1, 0x85 };
        var notification = new MUDHErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.NotInMultiUnit, notification.ErrorCode);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_NotMultiUnitBaseAddress()
    {
        var buffer = new byte[] { 0xE1, 0x86 };
        var notification = new MUDHErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.NotMultiUnitBaseAddress, notification.ErrorCode);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_CannotDelete()
    {
        var buffer = new byte[] { 0xE1, 0x87 };
        var notification = new MUDHErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.CannotDelete, notification.ErrorCode);
    }

    [TestMethod]
    public void MUDHErrorNotification_ParsesCorrectly_StackFull()
    {
        var buffer = new byte[] { 0xE1, 0x88 };
        var notification = new MUDHErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.StackFull, notification.ErrorCode);
        Assert.IsTrue(notification.ErrorMessage.Contains("stack is full"));
    }

    [TestMethod]
    public void MUDHErrorNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0xE1, 0x81 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(MUDHErrorNotification));
    }

    #endregion
}
