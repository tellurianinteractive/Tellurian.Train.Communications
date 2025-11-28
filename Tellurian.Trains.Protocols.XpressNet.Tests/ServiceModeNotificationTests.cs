using Tellurian.Trains.Protocols.XpressNet.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class ServiceModeNotificationTests
{
    #region ServiceModeRegisterPagedNotification

    [TestMethod]
    public void RegisterPagedNotification_ParsesCorrectly_ForRegister1()
    {
        var buffer = new byte[] { 0x63, 0x10, 0x01, 0x55, 0xFF };
        var notification = new ServiceModeRegisterPagedNotification(buffer);

        Assert.AreEqual(0x63, notification.Header);
        Assert.AreEqual(0x01, notification.RegisterOrCv);
        Assert.AreEqual(1, notification.CvNumber);
        Assert.AreEqual(0x55, notification.Value);
    }

    [TestMethod]
    public void RegisterPagedNotification_ParsesCorrectly_ForCV256()
    {
        var buffer = new byte[] { 0x63, 0x10, 0x00, 0xAA, 0xFF };
        var notification = new ServiceModeRegisterPagedNotification(buffer);

        Assert.AreEqual(0x00, notification.RegisterOrCv);
        Assert.AreEqual(256, notification.CvNumber);
        Assert.AreEqual(0xAA, notification.Value);
    }

    [TestMethod]
    public void RegisterPagedNotification_ParsesCorrectly_ForCV128()
    {
        var buffer = new byte[] { 0x63, 0x10, 0x80, 0x42, 0xFF };
        var notification = new ServiceModeRegisterPagedNotification(buffer);

        Assert.AreEqual(0x80, notification.RegisterOrCv);
        Assert.AreEqual(128, notification.CvNumber);
        Assert.AreEqual(0x42, notification.Value);
    }

    [TestMethod]
    public void RegisterPagedNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x63, 0x10, 0x01, 0x55, 0xFF };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(ServiceModeRegisterPagedNotification));
    }

    #endregion

    #region ServiceModeDirectCVNotification

    [TestMethod]
    public void DirectCVNotification_ParsesCorrectly_ForCV1()
    {
        var buffer = new byte[] { 0x63, 0x14, 0x01, 0x03, 0xFF };
        var notification = new ServiceModeDirectCVNotification(buffer);

        Assert.AreEqual(0x63, notification.Header);
        Assert.AreEqual(0x01, notification.CvByte);
        Assert.AreEqual(1, notification.CvNumber);
        Assert.AreEqual(0x03, notification.Value);
    }

    [TestMethod]
    public void DirectCVNotification_ParsesCorrectly_ForCV256()
    {
        var buffer = new byte[] { 0x63, 0x14, 0x00, 0xFF, 0xFF };
        var notification = new ServiceModeDirectCVNotification(buffer);

        Assert.AreEqual(0x00, notification.CvByte);
        Assert.AreEqual(256, notification.CvNumber);
        Assert.AreEqual(0xFF, notification.Value);
    }

    [TestMethod]
    public void DirectCVNotification_ParsesCorrectly_ForCV128()
    {
        var buffer = new byte[] { 0x63, 0x14, 0x80, 0x07, 0xFF };
        var notification = new ServiceModeDirectCVNotification(buffer);

        Assert.AreEqual(0x80, notification.CvByte);
        Assert.AreEqual(128, notification.CvNumber);
        Assert.AreEqual(0x07, notification.Value);
    }

    [TestMethod]
    public void DirectCVNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0x63, 0x14, 0x01, 0x03, 0xFF };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(ServiceModeDirectCVNotification));
    }

    #endregion

    #region Factory distinguishes between notification types

    [TestMethod]
    public void Factory_CreatesVersionNotification_ForHeader0x63Identification0x21()
    {
        var buffer = new byte[] { 0x63, 0x21, 0x30, 0x12, 0xFF };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType(notification, typeof(VersionNotification));
    }

    #endregion
}
