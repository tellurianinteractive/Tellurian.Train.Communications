using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AddressInquiryMultiUnitMemberCommandTests
{
    [TestMethod]
    public void AddressInquiryMultiUnitMember_ReturnsCorrectBytes_ForwardSearch()
    {
        var target = new AddressInquiryMultiUnitMemberCommand(10, Address.From(100));
        var data = target.GetData();

        Assert.AreEqual(0xE4, data[0]);
        Assert.AreEqual(0x01, data[1]);
        Assert.AreEqual(10, data[2]);
        Assert.AreEqual(0x00, data[3]);
        Assert.AreEqual(0x64, data[4]);
    }

    [TestMethod]
    public void AddressInquiryMultiUnitMember_ReturnsCorrectBytes_BackwardSearch()
    {
        var target = new AddressInquiryMultiUnitMemberCommand(10, Address.From(100), SearchDirection.Backward);
        var data = target.GetData();

        Assert.AreEqual(0x02, data[1]);
    }

    [TestMethod]
    public void AddressInquiryMultiUnitMember_ReturnsCorrectBytes_NoStartAddress()
    {
        var target = new AddressInquiryMultiUnitMemberCommand(10);
        var data = target.GetData();

        Assert.AreEqual(0x00, data[3]);
        Assert.AreEqual(0x00, data[4]);
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
}
