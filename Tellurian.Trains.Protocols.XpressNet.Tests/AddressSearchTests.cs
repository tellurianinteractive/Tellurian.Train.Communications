using Tellurian.Trains.Protocols.XpressNet.Commands;
using Tellurian.Trains.Protocols.XpressNet.Notifications;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AddressSearchTests
{
    #region AddressInquiryMultiUnitMemberCommand

    [TestMethod]
    public void AddressInquiryMultiUnitMember_ReturnsCorrectBytes_ForwardSearch()
    {
        var target = new AddressInquiryMultiUnitMemberCommand(10, new LocoAddress(100));
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);     // Header with length 4
        Assert.AreEqual(0x01, data[1]);     // Identification (forward search)
        Assert.AreEqual(10, data[2]);       // MTR address
        Assert.AreEqual(0x00, data[3]);     // Loco Address High (short address)
        Assert.AreEqual(0x64, data[4]);     // Loco Address Low (100)
    }

    [TestMethod]
    public void AddressInquiryMultiUnitMember_ReturnsCorrectBytes_BackwardSearch()
    {
        var target = new AddressInquiryMultiUnitMemberCommand(10, new LocoAddress(100), SearchDirection.Backward);
        var data = target.GetData();

        Assert.AreEqual(0x02, data[1]);     // Identification (backward search)
    }

    [TestMethod]
    public void AddressInquiryMultiUnitMember_ReturnsCorrectBytes_NoStartAddress()
    {
        var target = new AddressInquiryMultiUnitMemberCommand(10);
        var data = target.GetData();

        Assert.AreEqual(0x00, data[3]);     // Address High = 0
        Assert.AreEqual(0x00, data[4]);     // Address Low = 0
    }

    [TestMethod]
    public void AddressInquiryMultiUnitMember_Throws_WhenMTRInvalid()
    {
        try
        {
            _ = new AddressInquiryMultiUnitMemberCommand(0);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }

        try
        {
            _ = new AddressInquiryMultiUnitMemberCommand(100);
            Assert.Fail("Expected ArgumentOutOfRangeException");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    #endregion

    #region AddressInquiryMultiUnitCommand

    [TestMethod]
    public void AddressInquiryMultiUnit_ReturnsCorrectBytes_ForwardSearch()
    {
        var target = new AddressInquiryMultiUnitCommand(10);
        var data = target.GetData();

        Assert.AreEqual(0xE2, data[0]);     // Header with length 2
        Assert.AreEqual(0x03, data[1]);     // Identification (forward search)
        Assert.AreEqual(10, data[2]);       // MTR address
    }

    [TestMethod]
    public void AddressInquiryMultiUnit_ReturnsCorrectBytes_BackwardSearch()
    {
        var target = new AddressInquiryMultiUnitCommand(10, SearchDirection.Backward);
        var data = target.GetData();

        Assert.AreEqual(0x04, data[1]);     // Identification (backward search)
    }

    [TestMethod]
    public void AddressInquiryMultiUnit_ReturnsCorrectBytes_GetFirst()
    {
        var target = new AddressInquiryMultiUnitCommand(0);
        var data = target.GetData();

        Assert.AreEqual(0x00, data[2]);     // MTR = 0 to get first
    }

    #endregion

    #region AddressInquiryStackCommand

    [TestMethod]
    public void AddressInquiryStack_ReturnsCorrectBytes_ForwardSearch()
    {
        var target = new AddressInquiryStackCommand(new LocoAddress(1234));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);     // Header with length 3
        Assert.AreEqual(0x05, data[1]);     // Identification (forward search)
        Assert.AreEqual(0xC4, data[2]);     // Address High (long address)
        Assert.AreEqual(0xD2, data[3]);     // Address Low
    }

    [TestMethod]
    public void AddressInquiryStack_ReturnsCorrectBytes_BackwardSearch()
    {
        var target = new AddressInquiryStackCommand(new LocoAddress(3), SearchDirection.Backward);
        var data = target.GetData();

        Assert.AreEqual(0x06, data[1]);     // Identification (backward search)
    }

    [TestMethod]
    public void AddressInquiryStack_ReturnsCorrectBytes_GetFirst()
    {
        var target = new AddressInquiryStackCommand();
        var data = target.GetData();

        Assert.AreEqual(0x00, data[2]);     // Address High = 0
        Assert.AreEqual(0x00, data[3]);     // Address Low = 0
    }

    #endregion

    #region DeleteLocoFromStackCommand

    [TestMethod]
    public void DeleteLocoFromStack_ReturnsCorrectBytes_ShortAddress()
    {
        var target = new DeleteLocoFromStackCommand(new LocoAddress(3));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);     // Header with length 3
        Assert.AreEqual(0x44, data[1]);     // Identification
        Assert.AreEqual(0x00, data[2]);     // Address High
        Assert.AreEqual(0x03, data[3]);     // Address Low
    }

    [TestMethod]
    public void DeleteLocoFromStack_ReturnsCorrectBytes_LongAddress()
    {
        var target = new DeleteLocoFromStackCommand(new LocoAddress(5000));
        var data = target.GetData();

        Assert.AreEqual(0xE3, data[0]);
        Assert.AreEqual(0x44, data[1]);
        // Address 5000 = 0x1388, with long address flag -> 0xD3, 0x88
        Assert.AreEqual(0xD3, data[2]);
        Assert.AreEqual(0x88, data[3]);
    }

    #endregion

    #region AddressRetrievalNotification

    [TestMethod]
    public void AddressRetrievalNotification_ParsesCorrectly_NormalLoco()
    {
        // Header=0xE3, Identification=0x30 (K=0), Address=1234
        var buffer = new byte[] { 0xE3, 0x30, 0xC4, 0xD2 };
        var notification = new AddressRetrievalNotification(buffer);

        Assert.AreEqual(AddressType.NormalLoco, notification.AddressType);
        Assert.IsTrue(notification.AddressFound);
        Assert.IsNotNull(notification.LocoAddress);
        Assert.AreEqual(1234, notification.LocoAddress!.Value.Number);
    }

    [TestMethod]
    public void AddressRetrievalNotification_ParsesCorrectly_InDoubleHeader()
    {
        var buffer = new byte[] { 0xE3, 0x31, 0x00, 0x03 };
        var notification = new AddressRetrievalNotification(buffer);

        Assert.AreEqual(AddressType.InDoubleHeader, notification.AddressType);
        Assert.AreEqual(3, notification.LocoAddress!.Value.Number);
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

        Assert.AreEqual(AddressType.NotFound, notification.AddressType);
        Assert.IsFalse(notification.AddressFound);
        Assert.IsNull(notification.LocoAddress);
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

    #endregion

    #region LocoOperatedByAnotherDeviceNotification

    [TestMethod]
    public void LocoOperatedByAnotherDevice_ParsesCorrectly()
    {
        var buffer = new byte[] { 0xE3, 0x40, 0xC4, 0xD2 };
        var notification = new LocoOperatedByAnotherDeviceNotification(buffer);

        Assert.AreEqual(0xE3, notification.Header);
        Assert.AreEqual(1234, notification.LocoAddress.Number);
    }

    [TestMethod]
    public void LocoOperatedByAnotherDevice_ParsesCorrectly_ShortAddress()
    {
        var buffer = new byte[] { 0xE3, 0x40, 0x00, 0x03 };
        var notification = new LocoOperatedByAnotherDeviceNotification(buffer);

        Assert.AreEqual(3, notification.LocoAddress.Number);
    }

    [TestMethod]
    public void LocoOperatedByAnotherDevice_CreatedByFactory()
    {
        var buffer = new byte[] { 0xE3, 0x40, 0x00, 0x03 };
        var notification = NotificationFactory.Create(buffer);

        Assert.IsInstanceOfType<LocoOperatedByAnotherDeviceNotification>(notification);
    }

    #endregion

}
