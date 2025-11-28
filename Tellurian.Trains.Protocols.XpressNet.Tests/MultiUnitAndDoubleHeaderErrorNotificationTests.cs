using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class MultiUnitAndDoubleHeaderErrorNotificationTests
{
    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_ParsesCorrectly_NotOperatedByDevice()
    {
        var buffer = new byte[] { 0xE1, 0x81 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(0xE1, notification.Header);
        Assert.AreEqual(0x81, notification.IdentificationByte);
        Assert.AreEqual(MUDHErrorCode.NotOperatedByDevice, notification.ErrorCode);
        Assert.Contains("not been operated", notification.ErrorMessage);
    }

    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_ParsesCorrectly_OperatedByAnotherDevice()
    {
        var buffer = new byte[] { 0xE1, 0x82 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.OperatedByAnotherDevice, notification.ErrorCode);
        Assert.Contains("another XpressNet device", notification.ErrorMessage);
    }

    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_ParsesCorrectly_AlreadyInConsist()
    {
        var buffer = new byte[] { 0xE1, 0x83 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.AlreadyInConsist, notification.ErrorCode);
        Assert.Contains("already in another", notification.ErrorMessage);
    }

    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_ParsesCorrectly_SpeedNotZero()
    {
        var buffer = new byte[] { 0xE1, 0x84 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.SpeedNotZero, notification.ErrorCode);
        Assert.Contains("speed", notification.ErrorMessage);
    }

    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_ParsesCorrectly_NotInMultiUnit()
    {
        var buffer = new byte[] { 0xE1, 0x85 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.NotInMultiUnit, notification.ErrorCode);
    }

    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_ParsesCorrectly_NotMultiUnitBaseAddress()
    {
        var buffer = new byte[] { 0xE1, 0x86 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.NotMultiUnitBaseAddress, notification.ErrorCode);
    }

    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_ParsesCorrectly_CannotDelete()
    {
        var buffer = new byte[] { 0xE1, 0x87 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.CannotDelete, notification.ErrorCode);
    }

    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_ParsesCorrectly_StackFull()
    {
        var buffer = new byte[] { 0xE1, 0x88 };
        var notification = new MultiUnitAndDoubleHeaderErrorNotification(buffer);

        Assert.AreEqual(MUDHErrorCode.StackFull, notification.ErrorCode);
        Assert.Contains("stack is full", notification.ErrorMessage);
    }

    [TestMethod]
    public void MultiUnitAndDoubleHeaderErrorNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0xE1, 0x81 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<MultiUnitAndDoubleHeaderErrorNotification>(notification);
    }
}
