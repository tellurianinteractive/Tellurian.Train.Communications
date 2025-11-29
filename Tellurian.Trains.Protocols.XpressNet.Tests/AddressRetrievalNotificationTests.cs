using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AddressRetrievalNotificationTests
{
    [TestMethod]
    public void AddressRetrievalNotification_ParsesCorrectly_NormalLoco()
    {
        var buffer = new byte[] { 0xE3, 0x30, 0xC4, 0xD2 };
        var notification = new AddressRetrievalNotification(buffer);

        Assert.AreEqual(AddressType.NormalLoco, notification.AddressType);
        Assert.IsTrue(notification.HasValidAddress);
        Assert.AreEqual(1234, notification.LocoAddress!.Number);
    }

    [TestMethod]
    public void AddressRetrievalNotification_ParsesCorrectly_InDoubleHeader()
    {
        var buffer = new byte[] { 0xE3, 0x31, 0x00, 0x03 };
        var notification = new AddressRetrievalNotification(buffer);

        Assert.AreEqual(AddressType.InDoubleHeader, notification.AddressType);
        Assert.AreEqual(3, notification.LocoAddress!.Number);
    }

    [TestMethod]
    public void AddressRetrievalNotification_ParsesCorrectly_MultiUnitBase()
    {
        var buffer = new byte[] { 0xE3, 0x32, 0x00, 0x10 };
        var notification = new AddressRetrievalNotification(buffer);

        Assert.AreEqual(AddressType.MultiUnitBase, notification.AddressType);
    }

    [TestMethod]
    public void AddressRetrievalNotification_ParsesCorrectly_InMultiUnit()
    {
        var buffer = new byte[] { 0xE3, 0x33, 0x00, 0x05 };
        var notification = new AddressRetrievalNotification(buffer);

        Assert.AreEqual(AddressType.InMultiUnit, notification.AddressType);
    }

    [TestMethod]
    public void AddressRetrievalNotification_ParsesCorrectly_NotFound()
    {
        var buffer = new byte[] { 0xE3, 0x34, 0x00, 0x00 };
        var notification = new AddressRetrievalNotification(buffer);

        Assert.AreEqual(AddressType.Zero, notification.AddressType);
        Assert.IsFalse(notification.HasValidAddress);
        Assert.AreEqual(Address.Zero, notification.LocoAddress);
    }

    [TestMethod]
    public void AddressRetrievalNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0xE3, 0x30, 0x00, 0x03 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<AddressRetrievalNotification>(notification);
    }

    [TestMethod]
    public void AddressRetrievalNotification_CreatedByFactory_AllKValues()
    {
        for (byte k = 0; k <= 4; k++)
        {
            var buffer = new byte[] { 0xE3, (byte)(0x30 + k), 0x00, 0x03 };
            var notification = NotificationFactory.Create(buffer);
            Assert.IsInstanceOfType(notification, typeof(AddressRetrievalNotification), $"K={k} should create AddressRetrievalNotification");
        }
    }
}
