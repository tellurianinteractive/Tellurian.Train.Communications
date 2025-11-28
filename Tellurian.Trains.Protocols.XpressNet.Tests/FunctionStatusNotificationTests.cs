using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class FunctionStatusNotificationTests
{
    [TestMethod]
    public void FunctionStatusNotification_ParsesCorrectly_AllOnOff()
    {
        var buffer = new byte[] { 0xE3, 0x50, 0x1F, 0xFF };
        var notification = new FunctionStatusNotification(buffer);

        Assert.AreEqual(0xE3, notification.Header);
        Assert.AreEqual(0x1F, notification.Group1Status);
        Assert.AreEqual(0xFF, notification.Group2And3Status);

        for (int i = 0; i <= 12; i++)
        {
            Assert.IsTrue(notification.IsFunctionOnOff(i), $"F{i} should be on/off");
            Assert.IsFalse(notification.IsFunctionMomentary(i), $"F{i} should not be momentary");
        }
    }

    [TestMethod]
    public void FunctionStatusNotification_ParsesCorrectly_AllMomentary()
    {
        var buffer = new byte[] { 0xE3, 0x50, 0x00, 0x00 };
        var notification = new FunctionStatusNotification(buffer);

        for (int i = 0; i <= 12; i++)
        {
            Assert.IsFalse(notification.IsFunctionOnOff(i), $"F{i} should not be on/off");
            Assert.IsTrue(notification.IsFunctionMomentary(i), $"F{i} should be momentary");
        }
    }

    [TestMethod]
    public void FunctionStatusNotification_ParsesCorrectly_MixedStatus()
    {
        var buffer = new byte[] { 0xE3, 0x50, 0x10, 0x55 };
        var notification = new FunctionStatusNotification(buffer);

        Assert.IsTrue(notification.IsFunctionOnOff(0));
        Assert.IsFalse(notification.IsFunctionOnOff(1));
        Assert.IsFalse(notification.IsFunctionOnOff(2));
        Assert.IsFalse(notification.IsFunctionOnOff(3));
        Assert.IsFalse(notification.IsFunctionOnOff(4));

        Assert.IsTrue(notification.IsFunctionOnOff(5));
        Assert.IsFalse(notification.IsFunctionOnOff(6));
        Assert.IsTrue(notification.IsFunctionOnOff(7));
        Assert.IsFalse(notification.IsFunctionOnOff(8));
        Assert.IsTrue(notification.IsFunctionOnOff(9));
        Assert.IsFalse(notification.IsFunctionOnOff(10));
        Assert.IsTrue(notification.IsFunctionOnOff(11));
        Assert.IsFalse(notification.IsFunctionOnOff(12));
    }

    [TestMethod]
    public void FunctionStatusNotification_GetAllFunctionStates_ReturnsCorrectArray()
    {
        var buffer = new byte[] { 0xE3, 0x50, 0x15, 0xAA };
        var notification = new FunctionStatusNotification(buffer);

        var states = notification.GetAllFunctionStates();

        Assert.HasCount(13, states);
        Assert.IsTrue(states[0]);
        Assert.IsTrue(states[1]);
        Assert.IsFalse(states[2]);
        Assert.IsTrue(states[3]);
        Assert.IsFalse(states[4]);
    }

    [TestMethod]
    public void FunctionStatusNotification_Throws_ForInvalidFunctionNumber()
    {
        var buffer = new byte[] { 0xE3, 0x50, 0x00, 0x00 };
        var notification = new FunctionStatusNotification(buffer);

        try
        {
            _ = notification.IsFunctionOnOff(13);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }

        try
        {
            _ = notification.IsFunctionOnOff(-1);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void FunctionStatusNotification_CreatedByFactory()
    {
        var buffer = new byte[] { 0xE3, 0x50, 0x1F, 0xFF };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<FunctionStatusNotification>(notification);
    }
}
