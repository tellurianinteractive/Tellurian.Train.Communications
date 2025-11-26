using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class FunctionStatusTests
{
    #region GetFunctionStatusCommand

    [TestMethod]
    public void GetFunctionStatus_ReturnsCorrectBytes_ForShortAddress()
    {
        var target = new GetFunctionStatusCommand(new LocoAddress(3));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);     // Header with length 3
        Assert.AreEqual(0x07, data[1]);     // Identification
        Assert.AreEqual(0x00, data[2]);     // Address High (short address)
        Assert.AreEqual(0x03, data[3]);     // Address Low
    }

    [TestMethod]
    public void GetFunctionStatus_ReturnsCorrectBytes_ForLongAddress()
    {
        var target = new GetFunctionStatusCommand(new LocoAddress(1234));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);     // Header with length 3
        Assert.AreEqual(0x07, data[1]);     // Identification
        // Address 1234 = 0x04D2, with long address flag 0xC0 -> 0xC4, 0xD2
        Assert.AreEqual(0xC4, data[2]);     // Address High
        Assert.AreEqual(0xD2, data[3]);     // Address Low
    }

    #endregion

    #region SetFunctionStateGroup1Command

    [TestMethod]
    public void SetFunctionStateGroup1_ReturnsCorrectBytes_AllOnOff()
    {
        var target = new SetFunctionStateGroup1Command(new LocoAddress(3), true, true, true, true, true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);     // Header with length 4
        Assert.AreEqual(0x24, data[1]);     // Identification for Group 1
        Assert.AreEqual(0x00, data[2]);     // Address High
        Assert.AreEqual(0x03, data[3]);     // Address Low
        Assert.AreEqual(0x1F, data[4]);     // All bits set: 000_1_1111 = F0,F4,F3,F2,F1 all on/off
    }

    [TestMethod]
    public void SetFunctionStateGroup1_ReturnsCorrectBytes_AllMomentary()
    {
        var target = new SetFunctionStateGroup1Command(new LocoAddress(3), false, false, false, false, false);
        var data = target.GetData();

        Assert.AreEqual(0x00, data[4]);     // All bits clear = all momentary
    }

    [TestMethod]
    public void SetFunctionStateGroup1_ReturnsCorrectBytes_F0OnlyMomentary()
    {
        var target = new SetFunctionStateGroup1Command(new LocoAddress(3), false, true, true, true, true);
        var data = target.GetData();

        Assert.AreEqual(0x0F, data[4]);     // 000_0_1111 = F0 momentary, F1-F4 on/off
    }

    [TestMethod]
    public void SetFunctionStateGroup1_ReturnsCorrectBytes_WithStateByte()
    {
        var target = new SetFunctionStateGroup1Command(new LocoAddress(3), 0x15);
        var data = target.GetData();

        Assert.AreEqual(0x15, data[4]);     // Direct state byte: 000_1_0101
    }

    #endregion

    #region SetFunctionStateGroup2Command

    [TestMethod]
    public void SetFunctionStateGroup2_ReturnsCorrectBytes_AllOnOff()
    {
        var target = new SetFunctionStateGroup2Command(new LocoAddress(3), true, true, true, true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);     // Header with length 4
        Assert.AreEqual(0x25, data[1]);     // Identification for Group 2
        Assert.AreEqual(0x0F, data[4]);     // 0000_1111 = F8,F7,F6,F5 all on/off
    }

    [TestMethod]
    public void SetFunctionStateGroup2_ReturnsCorrectBytes_F5OnlyOnOff()
    {
        var target = new SetFunctionStateGroup2Command(new LocoAddress(3), true, false, false, false);
        var data = target.GetData();

        Assert.AreEqual(0x01, data[4]);     // 0000_0001 = F5 on/off, F6-F8 momentary
    }

    #endregion

    #region SetFunctionStateGroup3Command

    [TestMethod]
    public void SetFunctionStateGroup3_ReturnsCorrectBytes_AllOnOff()
    {
        var target = new SetFunctionStateGroup3Command(new LocoAddress(3), true, true, true, true);
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);     // Header with length 4
        Assert.AreEqual(0x26, data[1]);     // Identification for Group 3
        Assert.AreEqual(0x0F, data[4]);     // 0000_1111 = F12,F11,F10,F9 all on/off
    }

    [TestMethod]
    public void SetFunctionStateGroup3_ReturnsCorrectBytes_F12OnlyOnOff()
    {
        var target = new SetFunctionStateGroup3Command(new LocoAddress(3), false, false, false, true);
        var data = target.GetData();

        Assert.AreEqual(0x08, data[4]);     // 0000_1000 = F12 on/off, F9-F11 momentary
    }

    #endregion

    #region FunctionStatusNotification

    [TestMethod]
    public void FunctionStatusNotification_ParsesCorrectly_AllOnOff()
    {
        // Header=0xE3, Identification=0x50, S0=0x1F (all on/off), S1=0xFF (all on/off)
        var buffer = new byte[] { 0xE3, 0x50, 0x1F, 0xFF };
        var notification = new FunctionStatusNotification(buffer);

        Assert.AreEqual(0xE3, notification.Header);
        Assert.AreEqual(0x1F, notification.Group1Status);
        Assert.AreEqual(0xFF, notification.Group2And3Status);

        // All functions should be on/off (not momentary)
        for (int i = 0; i <= 12; i++)
        {
            Assert.IsTrue(notification.IsFunctionOnOff(i), $"F{i} should be on/off");
            Assert.IsFalse(notification.IsFunctionMomentary(i), $"F{i} should not be momentary");
        }
    }

    [TestMethod]
    public void FunctionStatusNotification_ParsesCorrectly_AllMomentary()
    {
        // All zeros = all momentary
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
        // S0=0x10 (F0 on/off, F1-F4 momentary), S1=0x55 (alternating)
        var buffer = new byte[] { 0xE3, 0x50, 0x10, 0x55 };
        var notification = new FunctionStatusNotification(buffer);

        // Group 1: F0=on/off, F1-F4=momentary
        Assert.IsTrue(notification.IsFunctionOnOff(0));
        Assert.IsFalse(notification.IsFunctionOnOff(1));
        Assert.IsFalse(notification.IsFunctionOnOff(2));
        Assert.IsFalse(notification.IsFunctionOnOff(3));
        Assert.IsFalse(notification.IsFunctionOnOff(4));

        // Group 2&3: 0x55 = 0101_0101 -> F5,F7,F9,F11 on/off; F6,F8,F10,F12 momentary
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
        // S0=0x15 = 0001_0101 -> F0=on/off, F1=momentary, F2=on/off, F3=momentary, F4=on/off
        Assert.IsTrue(states[0]);   // F0
        Assert.IsTrue(states[1]);   // F1 (bit 0)
        Assert.IsFalse(states[2]);  // F2 (bit 1)
        Assert.IsTrue(states[3]);   // F3 (bit 2)
        Assert.IsFalse(states[4]);  // F4 (bit 3)
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

    #endregion
}
