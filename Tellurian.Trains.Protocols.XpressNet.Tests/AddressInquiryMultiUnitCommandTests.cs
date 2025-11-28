using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Tests;

[TestClass]
public class AddressInquiryMultiUnitCommandTests
{
    [TestMethod]
    public void AddressInquiryMultiUnit_ReturnsCorrectBytes_ForwardSearch()
    {
        var target = new AddressInquiryMultiUnitCommand(10);
        var data = target.GetData();

        Assert.AreEqual(0xE2, data[0]);
        Assert.AreEqual(0x03, data[1]);
        Assert.AreEqual(10, data[2]);
    }

    [TestMethod]
    public void AddressInquiryMultiUnit_ReturnsCorrectBytes_BackwardSearch()
    {
        var target = new AddressInquiryMultiUnitCommand(10, SearchDirection.Backward);
        var data = target.GetData();

        Assert.AreEqual(0x04, data[1]);
    }

    [TestMethod]
    public void AddressInquiryMultiUnit_ReturnsCorrectBytes_GetFirst()
    {
        var target = new AddressInquiryMultiUnitCommand(0);
        var data = target.GetData();

        Assert.AreEqual(0x00, data[2]);
    }
}
